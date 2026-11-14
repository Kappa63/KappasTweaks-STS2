using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Blitz;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace KappasTweaks.KappasTweaksCode.Ancients;

public class KappaTheAncient : KappasTweaksAncient
{
    // HYPER
    private const int _blitzCardChoices = 10;
    private const int _blitzCardsToPick = 5;
    private const int _blitzStrikesToRemove = 1;
    private const int _blitzDefendsToRemove = 1;

    public override bool IsValidForAct(ActModel act)
    {
        return false;
    }

    protected override OptionPools MakeOptionPools => new OptionPools(
        [
            AncientOption<GottaCatchEmAll>(),
            AncientOption<HahaBomba>(),
            AncientOption<TheShining>(),
            AncientOption<FearTheReaper>(),
            AncientOption<FirstBlood>(),
        ]
    );

    private Player Player => Owner!;

    // Blitz Neow swaps its whole menu for a card pick; Kappa standing in for Neow needs his own version of that.
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        if (!Player.RunState.RunModels.OfType<BlitzRunModel>().Any())
            return base.GenerateInitialOptions();

        return
        [
            new EventOption(this, Blitz, L10NLookup($"{Id.Entry}.pages.INITIAL.options.BLITZ.title"),
                L10NLookup($"{Id.Entry}.pages.INITIAL.options.BLITZ.description"), "BLITZ", []),
        ];
    }

    private async Task Blitz()
    {
        List<CardModel> basics =
        [
            ..Basics(CardTag.Strike).Take(_blitzStrikesToRemove),
            ..Basics(CardTag.Defend).Take(_blitzDefendsToRemove),
        ];
        if (basics.Count > 0)
            await CardPileCmd.RemoveFromDeck(basics);

        CardCreationOptions options = new CardCreationOptions([Player.Character.CardPool], CardCreationSource.Other,
            CardRarityOddsType.BlitzNeow).WithFlags(CardCreationFlags.NoUpgrades);
        List<CardCreationResult> choices = CardFactory.CreateForReward(Player, _blitzCardChoices, options)
            .OrderBy(r => r.Card.Rarity)
            .ToList();

        CardSelectorPrefs prefs = new(L10NLookup($"{Id.Entry}.pages.INITIAL.options.BLITZ.prompt"), _blitzCardsToPick);
        List<CardModel> picked = (await CardSelectCmd.FromSimpleGridForRewards(new BlockingPlayerChoiceContext(),
            choices, Player, prefs)).ToList();

        foreach (CardModel card in picked)
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));

        foreach (CardCreationResult choice in choices.Where(c => !picked.Contains(c.Card)))
        {
            Player.RunState.CurrentMapPointHistoryEntry.GetEntry(Player.NetId).CardChoices
                .Add(new CardChoiceHistoryEntry(choice.Card, wasPicked: false));
        }

        Done();
    }

    private IEnumerable<CardModel> Basics(CardTag tag)
    {
        return Player.Deck.Cards.Where(c => c.Rarity == CardRarity.Basic && c.Tags.Contains(tag));
    }
}
