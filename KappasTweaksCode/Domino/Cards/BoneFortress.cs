using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class BoneFortress() : BonesmanCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    // HYPER
    private const int _block = 20;
    private const int _blockUpgrade = 6;
    private const int _perLink = 3;

    protected override (int Left, int Right)? FixedPips => (4, 4);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(_block),
        new CalculationExtraVar(_perLink),
        ScaledBlock(c => c.WillChain ? c.ChainCount : 0),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Block(play, ScaledBlockValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(_blockUpgrade);
    }
}
