using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class DealOut() : BonesmanCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    // HYPER
    private const int _cards = 2;
    private const int _cardsUpgrade = 1;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        List<CardModel> pool = Owner.Character.CardPool.AllCards.Where(c => c is BonesmanCard { Domino: true } && c.Rarity is not (CardRarity.Basic or CardRarity.Ancient)).ToList();
        for (int i = 0; i < DynamicVars.Cards.IntValue && pool.Count > 0; i++)
        {
            CardModel card = CombatState!.CreateCard(pool[Owner.RunState.Rng.CombatCardGeneration.NextInt(pool.Count)], Owner);
            card.EnergyCost.SetThisTurn(0);
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_cardsUpgrade);
    }
}
