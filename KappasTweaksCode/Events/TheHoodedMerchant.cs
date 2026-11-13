using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;

// HYPER
using TargetEnchantment = KappasTweaks.KappasTweaksCode.Enchantments.WannaSeeMeDoItAgain;
using AttackEnchantment = KappasTweaks.KappasTweaksCode.Enchantments.OneMoreTime;

namespace KappasTweaks.KappasTweaksCode.Events;

public class TheHoodedMerchant : KappasTweaksEvent
{
    // HYPER
    private const int _healAmount = 10;
    private const int _cardsToEnchant = 1;
    private const int _enchantAmount = 1;
    private const string _enchantmentKey = "EnchantmentName";
    private const string _attackEnchantmentKey = "AttackEnchantmentName";
    private const string _cardKey = "CardName";

    public override ActModel[] Acts =>
        [ModelDb.Act<Overgrowth>(), ModelDb.Act<Underdocks>(), ModelDb.Act<Hive>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(_healAmount),
        new StringVar(_enchantmentKey, ModelDb.Enchantment<TargetEnchantment>().Title.GetFormattedText()),
        new StringVar(_attackEnchantmentKey, ModelDb.Enchantment<AttackEnchantment>().Title.GetFormattedText()),
        new StringVar(_cardKey, ModelDb.Card<Polymerization>().Title),
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Enchant, HoverTipFactory.FromEnchantment<TargetEnchantment>(_enchantAmount)),
        Option(Encore, HoverTipFactory.FromEnchantment<AttackEnchantment>(_enchantAmount)),
        Option(Heal),
        // Option(Card)
    ];

    private Player Player => Owner!;

    private async Task Enchant()
    {
        await EnchantFromDeck<TargetEnchantment>();
        SetEventFinished(PageDescription("ENCHANT"));
    }

    // OneMoreTime overrides CanEnchantCardType to attacks only, and the selector filters on CanEnchant, so
    // nothing extra is needed here to keep this off skills and powers.
    private async Task Encore()
    {
        await EnchantFromDeck<AttackEnchantment>();
        SetEventFinished(PageDescription("ENCORE"));
    }

    private async Task EnchantFromDeck<T>() where T : Enchantments.KappasTweaksEnchantment
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, _cardsToEnchant);
        foreach (CardModel card in await CardSelectCmd.FromDeckForEnchantment(
                     Player, ModelDb.Enchantment<T>(), _enchantAmount, prefs))
        {
            CardCmd.Enchant<T>(card, _enchantAmount);
            CardCmd.Preview(card);
        }
    }
    
    private async Task Heal()
    {
        await CreatureCmd.Heal(Player.Creature, DynamicVars.Heal.IntValue);
        SetEventFinished(PageDescription("HEAL"));
    }
    
    // private async Task Card()
    // {
    //     CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2);
    //     List<CardModel> cardsAandB = (await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList();
    //
    //     Polymerization custom = Owner.RunState.CreateCard<Polymerization>(Owner);
    //     if (cardsAandB.Count >= 2)
    //         custom.SetContents(cardsAandB[0], cardsAandB[1]);
    //
    //     await CardPileCmd.RemoveFromDeck(cardsAandB);
    //     CardPileAddResult res = await CardPileCmd.Add(custom, PileType.Deck);
    //     CardCmd.PreviewCardPileAdd(res, 2f);
    //     SetEventFinished(PageDescription("CARD"));
    // }
}
