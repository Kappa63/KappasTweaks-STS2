using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Ossify() : BonesmanCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    // HYPER
    private const int _block = 3;
    private const int _blockUpgrade = 2;
    private const int _bonus = 3;

    protected override (int Left, int Right)? FixedPips => (0, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new BlockVar(_block, ValueProp.Move),
        new IntVar(BonusKey, _bonus),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Block(play, DynamicVars.Block.BaseValue + (chained ? Bonus : 0));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(_blockUpgrade);
    }
}
