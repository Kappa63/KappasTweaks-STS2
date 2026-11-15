using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class DefendBonesman() : BonesmanCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    // HYPER
    private const int _block = 5;
    private const int _blockUpgrade = 3;
    private const int _bonus = 3;

    protected override bool Rolled => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(_block),
        new CalculationExtraVar(_bonus),
        ScaledBlock(c => c.WillChain ? 1 : 0),
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
