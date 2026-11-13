using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Enchantments;

public sealed class OneMoreTime : KappasTweaksEnchantment
{
    // HYPER
    private const int _fatalsRequired = 5;
    private const int _replayAddCount = 1;
    private const string _fatalsKey = "Fatals";
    private const string _replayAddKey = "ReplayAdd";

    private int _fatals;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(_fatalsKey, _fatalsRequired),
        new IntVar(_replayAddKey, _replayAddCount),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ];

    private int FatalsRequired => DynamicVars[_fatalsKey].IntValue;

    private int ReplayAdd => DynamicVars[_replayAddKey].IntValue;

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

    public override bool ShowAmount => true;

    public override int DisplayAmount => Fatals;

    /// <summary>
    /// Every kill this card has ever landed, never reset. SavedProperty, which is what carries it through a
    /// reload -- EnchantmentModel.ToSerializable stores SavedProperties.From(this).
    /// </summary>
    [SavedProperty]
    public int Fatals
    {
        get => _fatals;
        set
        {
            AssertMutable();
            _fatals = value;
        }
    }

    /// <summary>
    /// Derived from Fatals rather than incrementing Card.BaseReplayCount. That property carries no
    /// SavedProperty, so a bonus written into it would be lost on reload; recomputing from saved state is
    /// what actually makes this permanent.
    /// </summary>
    public override int EnchantPlayCount(int originalPlayCount) =>
        originalPlayCount + Fatals / FatalsRequired * ReplayAdd;

    public override Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (cardSource != Card || !result.WasTargetKilled)
            return Task.CompletedTask;

        // Feed's guard: some deaths aren't Fatal, and the power on the dying creature is what decides.
        if (!target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal()))
            return Task.CompletedTask;

        RecordFatal();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Counts the kill on this card and on its deck counterpart. The card in combat is a clone -- the Deck
    /// pile is re-cloned every combat and combat-side changes are discarded -- so without the DeckVersion
    /// half this would reset every fight. TheScythe and GeneticAlgorithm buff DeckVersion the same way.
    /// </summary>
    private void RecordFatal()
    {
        Fatals++;

        if (Card.DeckVersion?.Enchantment is OneMoreTime deck && !ReferenceEquals(deck, this))
            deck.Fatals++;
    }
}
