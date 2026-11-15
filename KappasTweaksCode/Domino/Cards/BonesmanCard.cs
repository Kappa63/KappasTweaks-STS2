using BaseLib.Extensions;
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
    private bool ForcedChain => Owner.Creature.GetPower<PerfectMatchPower>() != null || Owner.Creature.GetPower<SafetyNetPower>() != null;
    private bool _playing;
    private int _linksBefore;

    public static event Action<BonesmanCard>? PipsChanged;

    // Canonical cards (compendium, previews) have no owner, and asking for one throws.
    public int GlowHalf => IsMutable && IsDomino && PipLeft != _unset
        ? Chain.Of(Owner)?.LinkingHalf(PipLeft, PipRight, Tolerance, ForcedChain) ?? -1
        : -1;

    // Matches an open end by its pips alone, without spending a Perfect Match or Safety Net.
    public bool Matches => IsMutable && IsDomino && PipLeft != _unset && Chain.Of(Owner) is { } chain && chain.CanChain(PipLeft, PipRight, Tolerance);

    // The values below drive both the numbers printed on the card in hand and the effect when played, so the card
    // always shows what it will actually do. Before the play they predict; during it they read the real outcome.
    private ChainPower? LiveChain => IsMutable && CombatState != null ? Chain.Of(Owner) : null;
    public bool WillChain => _playing ? Outcome == ChainOutcome.Chained : GlowHalf >= 0;
    public bool WillBreak => _playing ? Outcome == ChainOutcome.Broke : IsDomino && LiveChain is { HasEnds: true } && GlowHalf < 0;
    // Dominoes in the chain counting this one if it Chains, or the chain it broke if it didn't.
    public int ChainCount => LiveChain is not { } chain ? 0 : _playing ? Math.Max(chain.Links, _linksBefore) : chain.Links + (WillChain ? 1 : 0);
    public int ChainedThisTurn => LiveChain is not { } chain ? 0 : chain.LinksThisTurn + (!_playing && WillChain ? 1 : 0);
    public int BreakCount => LiveChain is not { } chain ? 0 : chain.Breaks + (!_playing && WillBreak ? 1 : 0);
    public int ChipsPlayed => LiveChain?.ChipsPlayed ?? 0;
    public int HandPips => IsMutable && CombatState != null
        ? Owner.PlayerCombatState!.Hand.Cards.OfType<BonesmanCard>().Where(c => c != this && c.Domino && c.PipLeft >= 0).Sum(c => c.PipValue)
        : 0;
    public int PipValue => Pip(PipLeft) + Pip(PipRight);
    private static int Pip(int pip) => pip is >= 0 and <= ChainPower.MaxPip ? pip : 0;

    protected ChainOutcome Outcome { get; private set; }
    private bool HasPips => _pipLeft != _unset;
    public bool Domino => IsDomino;
    public bool IsDouble => PipLeft >= 0 && PipLeft == PipRight;
    public bool HasWild => PipLeft == ChainPower.Wild || PipRight == ChainPower.Wild;

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

    protected int PipTotal => PipValue;

    private string PipText => !IsDomino ? "" : PipLeft == _unset ? "? | ?" : $"{ChainPower.PipName(PipLeft)} | {ChainPower.PipName(PipRight)}";

    protected virtual string ArtName => Id.Entry.RemovePrefix().ToLowerInvariant();
    public override string PortraitPath => BonesmanArt.Portrait(ArtName, GetType().Name, big: false);
    public override string CustomPortraitPath => BonesmanArt.Portrait(ArtName, GetType().Name, big: true);

    protected sealed override IEnumerable<DynamicVar> CanonicalVars => [..CardVars, new StringVar(PipsKey, PipText)];

    protected sealed override IEnumerable<IHoverTip> ExtraHoverTips =>
        IsDomino ? [..CardHoverTips, HoverTipFactory.FromKeyword(BonesmanKeywords.Chained)] : CardHoverTips;

    protected override bool ShouldGlowGoldInternal =>
        GlowHalf >= 0;

    public override void AfterCreated()
    {
        base.AfterCreated();
        // Jumbled cards made straight into a pile never get drawn, so they need pips from the start.
        if ((Rolled || IsJumbled) && !HasPips)
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
        _playing = true;
        try
        {
            await PlayDomino(choiceContext, play);
        }
        finally
        {
            _playing = false;
        }
    }

    private async Task PlayDomino(PlayerChoiceContext choiceContext, CardPlay play)
    {
        Outcome = ChainOutcome.Started;
        _linksBefore = Chain.Of(Owner)?.Links ?? 0;
        if (IsDomino)
        {
            if (PipLeft == _unset)
                Roll(Owner.RunState.Rng.CombatCardGeneration);
            ChainPower chain = await Chain.Ensure(choiceContext, Owner);
            // A forced chain spends a Perfect Match before a Safety Net.
            PowerModel? rescue = !chain.HasEnds || chain.CanChain(PipLeft, PipRight, Tolerance)
                ? null
                : Owner.Creature.GetPower<PerfectMatchPower>() as PowerModel ?? Owner.Creature.GetPower<SafetyNetPower>();
            Outcome = chain.Place(PipLeft, PipRight, Tolerance, rescue != null,
                Owner.Creature.GetPower<EndlessChainPower>() != null);
            if (rescue != null)
                await PowerCmd.Decrement(rescue);
        }
        await OnDominoPlay(choiceContext, play, Outcome == ChainOutcome.Chained);

        if (!IsDomino)
            return;
        foreach (IChainListener listener in Listeners)
        {
            await listener.OnDominoPlayed(choiceContext, this);
            if (Outcome == ChainOutcome.Chained) await listener.OnChained(choiceContext, this);
            else if (Outcome == ChainOutcome.Broke) await listener.OnChainBroken(choiceContext, this);
        }
    }

    private List<IChainListener> Listeners => [..Owner.Creature.Powers.OfType<IChainListener>(), ..Owner.Relics.OfType<IChainListener>()];

    // Breaks the chain on purpose: the streak resets and everything that pays off on a break triggers.
    protected async Task BreakChain(PlayerChoiceContext context)
    {
        if (Chain.Of(Owner) is not { } chain)
            return;
        chain.Break();
        foreach (IChainListener listener in Listeners)
            await listener.OnChainBroken(context, this);
    }

    protected Task<IEnumerable<CardModel>> AddChips(int count) => BoneChip.CreateInHand(Owner, count, CombatState!);

    // Card numbers that move with the chain. The multiplier gets the card itself, so it reads that copy's state.
    protected static CalculatedDamageVar ScaledDamage(Func<BonesmanCard, decimal> multiplier) =>
        (CalculatedDamageVar)new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => multiplier((BonesmanCard)card));

    protected static CalculatedBlockVar ScaledBlock(Func<BonesmanCard, decimal> multiplier) =>
        (CalculatedBlockVar)new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, _) => multiplier((BonesmanCard)card));

    protected decimal ScaledDamageOn(Creature? target) => DynamicVars.CalculatedDamage.Calculate(target);
    protected decimal ScaledBlockValue => DynamicVars.CalculatedBlock.Calculate(null);

    protected Task HitScaled(PlayerChoiceContext context, CardPlay play, int hits = 1) =>
        Hit(context, play, ScaledDamageOn(play.Target), hits);

    protected Task HitAllScaled(PlayerChoiceContext context, CardPlay play) => HitAll(context, play, ScaledDamageOn(null));

    protected abstract Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained);

    internal void MakeDouble()
    {
        int high = Math.Max(Pip(PipLeft), Pip(PipRight));
        PipLeft = high;
        PipRight = high;
    }

    internal void Reroll()
    {
        if (IsDomino && FixedPips == null)
            Roll(Owner.RunState.Rng.CombatCardGeneration);
    }

    protected void Roll(Rng rng)
    {
        int left = rng.NextInt(ChainPower.Blank, ChainPower.MaxPip + 1);
        int right = rng.NextInt(ChainPower.Blank, ChainPower.MaxPip + 1);
        if (Owner.Creature.GetPower<WeightedDicePower>() != null && Chain.Of(Owner) is { HasEnds: true } chain)
            left = rng.NextInt(2) == 0 ? chain.Left : chain.Right;
        PipLeft = left;
        PipRight = right;
    }

    protected virtual void OnPipsChanged()
    {
    }

    private void RefreshPips()
    {
        if (DynamicVars.TryGetValue(PipsKey, out DynamicVar? var) && var is StringVar pips)
            pips.StringValue = PipText;
        OnPipsChanged();
        PipsChanged?.Invoke(this);
    }

    protected decimal Bonus => DynamicVars[BonusKey].BaseValue;

    protected Task Hit(PlayerChoiceContext context, CardPlay play, decimal damage, int hits = 1)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        return DamageCmd.Attack(damage).FromCard(this, play).Targeting(play.Target).WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(context);
    }

    protected Task HitAll(PlayerChoiceContext context, CardPlay play, decimal damage, int hits = 1)
    {
        return DamageCmd.Attack(damage).FromCard(this, play).TargetingAllOpponents(CombatState!).WithHitCount(hits)
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
