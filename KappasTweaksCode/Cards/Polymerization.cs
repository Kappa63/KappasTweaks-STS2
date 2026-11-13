using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace KappasTweaks.KappasTweaksCode.Cards;


[Pool(typeof(ColorlessCardPool))]
public class Polymerization() : KappasTweaksCard(1, CardType.Skill, CardRarity.Event, TargetType.None)
{
    // HYPER
    private const string _cardAKey = "CardA";
    private const string _cardBKey = "CardB";

    private SerializableCard? _cardA;
    private SerializableCard? _cardB;
    private int _resolvedCost;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar(_cardAKey, ModelDb.Card<Void>().Title),
        new StringVar(_cardBKey, ModelDb.Card<Void>().Title),
    ];

    [SavedProperty]
    public SerializableCard? CardA
    {
        get => _cardA;
        set
        {
            AssertMutable();
            _cardA = value;
        }
    }

    [SavedProperty]
    public SerializableCard? CardB
    {
        get => _cardB;
        set
        {
            AssertMutable();
            _cardB = value;
        }
    }

    [SavedProperty]
    public int ResolvedCost
    {
        get => _resolvedCost;
        set
        {
            AssertMutable();
            _resolvedCost = value;
        }
    }

    protected override int CanonicalEnergyCost => CardA != null ? ResolvedCost : base.CanonicalEnergyCost;

    public void SetContents(CardModel a, CardModel b)
    {
        CardA = a.ToSerializable();
        CardB = b.ToSerializable();
        ResolvedCost = Math.Max(
            a.EnergyCost.GetWithModifiers(CostModifiers.None),
            b.EnergyCost.GetWithModifiers(CostModifiers.None));
        EnergyCost.SetCustomBaseCost(ResolvedCost);
        RefreshNames();
    }

    protected override void AfterDeserialized()
    {
        base.AfterDeserialized();
        RefreshNames();
    }

    private void RefreshNames()
    {
        ((StringVar)DynamicVars[_cardAKey]).StringValue = DisplayName(CardA);
        ((StringVar)DynamicVars[_cardBKey]).StringValue = DisplayName(CardB);
    }

    private static string DisplayName(SerializableCard? card)
    {
        if (card?.Id is not { } id)
            return ModelDb.Card<Void>().Title;

        CardModel? model = ModelDb.GetByIdOrNull<CardModel>(id);
        string name = model?.TitleLocString.GetFormattedText() ?? id.Entry;

        if (card.CurrentUpgradeLevel > 0)
            name += model is { MaxUpgradeLevel: > 1 } ? $"+{card.CurrentUpgradeLevel}" : "+";

        if (card.Enchantment?.Id is { } enchId
            && ModelDb.GetByIdOrNull<EnchantmentModel>(enchId)?.Title.GetFormattedText() is { Length: > 0 } ench)
            name += $" ({ench})";

        return name;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        (SerializableCard? first, SerializableCard? second) = Owner.RunState.Rng.CombatCardSelection.NextBool()
            ? (CardA, CardB)
            : (CardB, CardA);

        await PlayStored(choiceContext, first);
        await PlayStored(choiceContext, second);
    }

    private async Task PlayStored(PlayerChoiceContext choiceContext, SerializableCard? content)
    {
        if (content == null || CardScope is not { } scope)
            return;

        CardModel card = CardModel.FromSerializable(content);
        scope.AddCard(card, Owner);

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Top);
        await CardPileCmd.AutoPlayFromDrawPile(choiceContext, Owner, 1, CardPilePosition.Top, forceExhaust: true);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}