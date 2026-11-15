using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class ResetTheBoard() : BonesmanCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    // HYPER
    private const int _cards = 3;
    private const int _cardsUpgrade = 1;

    protected override bool IsDomino => false;
    protected override IEnumerable<IHoverTip> CardHoverTips => [HoverTipFactory.FromKeyword(BonesmanKeywords.Wild)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await CardPileCmd.Shuffle(choiceContext, Owner);
        await Draw(choiceContext, DynamicVars.Cards.BaseValue);
        (await Chain.Ensure(choiceContext, Owner)).SetEnds(ChainPower.Wild, ChainPower.Wild);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_cardsUpgrade);
    }
}
