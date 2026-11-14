using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class MatchPoint() : BonesmanCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    // HYPER
    private const int _max = 4;
    private const int _maxUpgrade = 2;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_max),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        ChainPower chain = await Chain.Ensure(choiceContext, Owner);
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            if (await CardPileCmd.Draw(choiceContext, Owner) is not BonesmanCard drawn)
                continue;
            if (drawn.PipLeft >= 0 && chain.CanChain(drawn.PipLeft, drawn.PipRight))
                break;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_maxUpgrade);
    }
}
