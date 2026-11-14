using KappasTweaks.KappasTweaksCode.Cards;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

// Pips come from one of three places: FixedPips (printed on the card), Rolled (each copy rolls once when created and
// keeps them) or the Jumbled keyword (rerolls every draw). Cards without pips (IsDomino false) never touch the chain.
public abstract class BonesmanCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : KappasTweaksCard(cost, type, rarity, target)
{
    protected const string PipsKey = "Pips";
    protected const string BonusKey = "Bonus";
    private const int _unset = -1;

    private int _pipLeft = _unset;
    private int _pipRight = _unset;

    protected virtual (int Left, int Right)? FixedPips => null;
    protected virtual bool Rolled => false;
    protected virtual bool IsDomino => true;
    protected virtual IEnumerable<DynamicVar> CardVars => [];
    protected virtual IEnumerable<IHoverTip> CardHoverTips => [];

    private bool IsJumbled => CanonicalKeywords.Contains(BonesmanKeywords.Jumbled);
    private int Tolerance => CanonicalKeywords.Contains(BonesmanKeywords.Loose) || Owner.Creature.GetPower<LooseBonesPower>() != null ? 1 : 0;
    private bool AlwaysChains => Owner.Creature.GetPower<BoneyardKingPower>() != null;

    protected ChainOutcome Outcome { get; private set; }
    private bool HasPips => _pipLeft != _unset;

    [SavedProperty]
    public int PipLeft
    {
        get => HasPips ? _pipLeft : FixedPips?.Left ?? _unset;
        set
        {
            AssertMutable();
            _pipLeft = value;
            RefreshPips();
        }
    }

    [SavedProperty]
    public int PipRight
    {
        get => HasPips ? _pipRight : FixedPips?.Right ?? _unset;
        set
        {
            AssertMutable();
            _pipRight = value;
            RefreshPips();
        }
    }

    protected int PipTotal => Math.Max(PipLeft, 0) + Math.Max(PipRight, 0);

    private string PipText => !IsDomino ? "" : PipLeft == _unset ? "? | ?" : $"{PipLeft} | {PipRight}";

    protected sealed override IEnumerable<DynamicVar> CanonicalVars => [..CardVars, new StringVar(PipsKey, PipText)];

    protected sealed override IEnumerable<IHoverTip> ExtraHoverTips =>
        IsDomino ? [..CardHoverTips, HoverTipFactory.FromKeyword(BonesmanKeywords.Chained)] : CardHoverTips;

    protected override bool ShouldGlowGoldInternal =>
        IsDomino && PipLeft != _unset && Chain.Of(Owner)?.CanChain(PipLeft, PipRight, Tolerance, AlwaysChains) == true;

    public override void AfterCreated()
    {
        base.AfterCreated();
        if (Rolled && !HasPips)
            Roll(Owner.RunState.Rng.Niche);
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this && IsJumbled)
            Roll(Owner.RunState.Rng.CombatCardGeneration);
        return Task.CompletedTask;
    }

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        Outcome = ChainOutcome.Started;
        if (IsDomino)
        {
            if (PipLeft == _unset)
                Roll(Owner.RunState.Rng.CombatCardGeneration);
            Outcome = (await Chain.Ensure(choiceContext, Owner)).Place(PipLeft, PipRight, Tolerance, AlwaysChains);
        }
        await OnDominoPlay(choiceContext, play, Outcome == ChainOutcome.Chained);

        if (Outcome == ChainOutcome.Started)
            return;
        List<IChainListener> listeners = [..Owner.Creature.Powers.OfType<IChainListener>(), ..Owner.Relics.OfType<IChainListener>()];
        foreach (IChainListener listener in listeners)
        {
            if (Outcome == ChainOutcome.Chained) await listener.OnChained(choiceContext, this);
            else await listener.OnChainBroken(choiceContext, this);
        }
    }

    protected abstract Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained);

    internal void Reroll()
    {
        if (IsDomino && FixedPips == null)
            Roll(Owner.RunState.Rng.CombatCardGeneration);
    }

    protected void Roll(Rng rng)
    {
        PipLeft = rng.NextInt(ChainPower.Blank, ChainPower.MaxPip + 1);
        PipRight = rng.NextInt(ChainPower.Blank, ChainPower.MaxPip + 1);
    }

    protected virtual void OnPipsChanged()
    {
    }

    private void RefreshPips()
    {
        if (DynamicVars.TryGetValue(PipsKey, out DynamicVar? var) && var is StringVar pips)
            pips.StringValue = PipText;
        OnPipsChanged();
    }

    protected decimal Bonus => DynamicVars[BonusKey].BaseValue;

    protected Task Hit(PlayerChoiceContext context, CardPlay play, decimal damage, int hits = 1)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        return DamageCmd.Attack(damage).FromCard(this, play).Targeting(play.Target).WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(context);
    }

    protected Task HitAll(PlayerChoiceContext context, CardPlay play, decimal damage)
    {
        return DamageCmd.Attack(damage).FromCard(this, play).TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(context);
    }

    protected Task Block(CardPlay play, decimal amount) =>
        CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Move, play);

    protected Task Draw(PlayerChoiceContext context, decimal count) => CardPileCmd.Draw(context, count, Owner);

    protected Creature? RandomEnemy() =>
        CombatState?.HittableEnemies.ToList() is { Count: > 0 } enemies
            ? enemies[Owner.RunState.Rng.CombatTargets.NextInt(enemies.Count)]
            : null;
}
