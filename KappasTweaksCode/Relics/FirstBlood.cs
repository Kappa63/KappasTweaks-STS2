using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KappasTweaks.KappasTweaksCode.Relics;

[Pool(typeof(SharedRelicPool))]
public sealed class FirstBlood : KappasTweaksRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private bool _spentThisCombat;
    private bool _redirecting;
    private CardPlay? _firstAttackPlay;

    [SavedProperty]
    public bool SpentThisCombat
    {
        get => _spentThisCombat;
        set
        {
            AssertMutable();
            _spentThisCombat = value;
        }
    }

    // True only between BeforeCardPlayed and AfterCardPlayed of the first attack -- i.e. while that card's
    // OnPlay runs. Both patches key off it, so the whole card (damage AND its debuffs) hits every enemy.
    internal bool Redirecting
    {
        get => _redirecting;
        private set
        {
            AssertMutable();
            _redirecting = value;
        }
    }

    public override Task BeforeCombatStart()
    {
        SpentThisCombat = false;
        Redirecting = false;
        _firstAttackPlay = null;
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (SpentThisCombat || cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack)
            return Task.CompletedTask;

        SpentThisCombat = true;
        _firstAttackPlay = cardPlay;
        Redirecting = true;
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ReferenceEquals(cardPlay, _firstAttackPlay))
            Redirecting = false;

        return Task.CompletedTask;
    }
}

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Targeting))]
internal static class FirstBloodTargetingPatch
{
    [HarmonyPrefix]
    private static bool RedirectToEveryEnemy(AttackCommand __instance, Creature target, ref AttackCommand __result)
    {
        if (__instance.ModelSource is not CardModel card)
            return true;

        if (__instance.CardPlay == null || __instance.Attacker == null)
            return true;

        if (target.Side != CombatSide.Enemy)
            return true;

        ICombatState? combatState = card.CombatState;
        if (combatState == null)
            return true;

        FirstBlood? relic = card.Owner.GetRelic<FirstBlood>();
        if (relic == null || !relic.Redirecting)
            return true;

        __result = __instance.TargetingAllOpponents(combatState);
        return false;
    }
}

[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply),
    new[]
    {
        typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature),
        typeof(CardModel), typeof(bool),
    })]
internal static class FirstBloodPowerPatch
{
    // Combat is sequential on one thread, so a plain flag is enough to stop the fan-out re-entering itself.
    private static bool _fanningOut;

    [HarmonyPrefix]
    private static bool FanToEveryEnemy(PlayerChoiceContext choiceContext, PowerModel power, Creature target,
        decimal amount, Creature? applier, CardModel? cardSource, bool silent, ref Task __result)
    {
        if (_fanningOut || target.Side != CombatSide.Enemy)
            return true;

        FirstBlood? relic = cardSource?.Owner?.GetRelic<FirstBlood>();
        if (relic == null || !relic.Redirecting)
            return true;

        __result = FanOut(choiceContext, power, target, amount, applier, cardSource, silent);
        return false;
    }

    // The passed power was a blank fresh clone for the single target; discard it and give each enemy its own
    // (a power instance belongs to one creature). Apply handles new-vs-stacking per enemy internally.
    private static async Task FanOut(PlayerChoiceContext choiceContext, PowerModel power, Creature target,
        decimal amount, Creature? applier, CardModel? cardSource, bool silent)
    {
        _fanningOut = true;
        try
        {
            ICombatState? combatState = target.CombatState;
            if (combatState == null)
                return;

            foreach (Creature enemy in combatState.HittableEnemies.ToList())
            {
                PowerModel? fresh = ModelDb.GetByIdOrNull<PowerModel>(power.Id)?.ToMutable();
                if (fresh != null)
                    await PowerCmd.Apply(choiceContext, fresh, enemy, amount, applier, cardSource, silent);
            }
        }
        finally
        {
            _fanningOut = false;
        }
    }
}