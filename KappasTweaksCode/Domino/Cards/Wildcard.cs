using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Wildcard() : BonesmanCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    // HYPER
    private const int _cards = 1;
    private const int _cardsUpgrade = 1;

    protected override bool IsDomino => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        ChainPower chain = await Chain.Ensure(choiceContext, Owner);
        chain.SetEnds(ChainPower.Blank, chain.HasEnds ? chain.Right : ChainPower.Blank);
        await Draw(choiceContext, DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_cardsUpgrade);
    }
}
