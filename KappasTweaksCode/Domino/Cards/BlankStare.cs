using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

// Breaker: cash the streak in for Block.
[Pool(typeof(BonesmanCardPool))]
public sealed class BlankStare() : BonesmanCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    // HYPER
    private const int _block = 4;
    private const int _blockUpgrade = 3;
    private const int _perLink = 2;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(_block),
        new CalculationExtraVar(_perLink),
        ScaledBlock(c => c.ChainCount),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Block(play, ScaledBlockValue);
        await BreakChain(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(_blockUpgrade);
    }
}
