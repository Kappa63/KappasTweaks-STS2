using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Tally() : BonesmanCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    // HYPER
    private const int _per = 3;
    private const int _perUpgrade = -1;
    private const string _drawKey = "Draws";

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new IntVar(BonusKey, _per),
        new LiveVar(_drawKey, c => 1 + c.ChainCount / c.DynamicVars[BonusKey].IntValue),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Draw(choiceContext, ((LiveVar)DynamicVars[_drawKey]).Of(this));
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusKey].UpgradeValueBy(_perUpgrade);
    }
}
