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

// Built only on game hooks plus a field write. Harmony patches on tiny methods like AttackCommand.Targeting can be
// skipped once the runtime inlines them, and in multiplayer that skip happens on one machine but not the other.
[Pool(typeof(SharedRelicPool))]
public sealed class FirstBlood : KappasTweaksRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private static readonly AccessTools.FieldRef<AttackCommand, Creature?> _singleTarget =
        AccessTools.FieldRefAccess<AttackCommand, Creature?>("_singleTarget");

    private static readonly AccessTools.FieldRef<AttackCommand, ICombatState?> _opponentsOf =
        AccessTools.FieldRefAccess<AttackCommand, ICombatState?>("_combatState");

    private bool _spentThisCombat;
    private bool _redirecting;
    private bool _fanningOut;
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

    // True only while the first attack's OnPlay runs, so the whole card (damage AND its debuffs) hits every enemy.
    private bool Redirecting
    {
        get => _redirecting;
        set
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

    // Fires before the attack picks its targets, and every hit re-reads them, so retargeting here covers all hits.
    public override Task BeforeAttack(AttackCommand command)
    {
        if (Redirecting && command.ModelSource is CardModel card && card.Owner == Owner && card.CombatState is { } combat
            && _singleTarget(command) is { Side: CombatSide.Enemy })
        {
            _singleTarget(command) = null;
            _opponentsOf(command) = combat;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!Redirecting || _fanningOut || amount <= 0 || cardSource?.Owner != Owner
            || power.Owner is not { Side: CombatSide.Enemy } target || target.CombatState is not { } combat)
        {
            return;
        }

        // A power instance belongs to one creature, so each other enemy gets a fresh copy.
        _fanningOut = true;
        try
        {
            foreach (Creature enemy in combat.HittableEnemies.Where(e => e != target).ToList())
            {
                if (ModelDb.GetByIdOrNull<PowerModel>(power.Id)?.ToMutable() is { } fresh)
                    await PowerCmd.Apply(choiceContext, fresh, enemy, amount, applier, cardSource);
            }
        }
        finally
        {
            _fanningOut = false;
        }
    }
}
