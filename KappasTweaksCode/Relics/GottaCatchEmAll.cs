using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Relics;

[Pool(typeof(SharedRelicPool))]
public sealed class GottaCatchEmAll : KappasTweaksRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool TryModifyCardRewardOptions(
        Player player,
        List<CardCreationResult> cardRewardOptions,
        CardCreationOptions creationOptions)
    {
        if (Owner != player)
            return false;

        if (creationOptions.Source != CardCreationSource.Encounter)
            return false;

        if (!creationOptions.Flags.HasFlag(CardCreationFlags.IsCardReward))
            return false;

        if (!creationOptions.Flags.HasFlag(CardCreationFlags.IsFromCombat))
            return false;

        if (creationOptions.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
            return false;

        List<CardCreationResult> offers = [];
        foreach (CardPoolModel pool in ModelDb.AllCharacterCardPools)
        {
            CardCreationResult? offer = CreateOfferFrom(player, pool, creationOptions);
            if (offer != null)
                offers.Add(offer);
        }

        if (offers.Count == 0)
            return false;

        cardRewardOptions.Clear();
        cardRewardOptions.AddRange(offers);
        return true;
    }

    private CardCreationResult? CreateOfferFrom(Player player, CardPoolModel pool, CardCreationOptions creationOptions)
    {
        CardCreationOptions options = new CardCreationOptions(
                [pool],
                creationOptions.Source,
                creationOptions.RarityOdds,
                creationOptions.CardPoolFilter)
            .WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);

        if (!options.GetPossibleCards(player).Any())
            return null;

        CardModel? card = CardFactory.CreateForReward(player, 1, options).FirstOrDefault()?.Card;
        if (card == null)
            return null;

        CardCreationResult offer = new CardCreationResult(card);
        offer.ModifyCard(card, this);
        return offer;
    }
}
