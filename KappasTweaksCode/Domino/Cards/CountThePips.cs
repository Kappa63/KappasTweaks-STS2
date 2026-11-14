using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class CountThePips() : BonesmanCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    // HYPER
    private const int _flat = 3;
    private const int _flatUpgrade = 3;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new IntVar(BonusKey, _flat),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        ChainPower? chain = Chain.Of(Owner);
        int ends = chain is { HasEnds: true } ? chain.Left + chain.Right : 0;
        await Block(play, ends + Bonus);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusKey].UpgradeValueBy(_flatUpgrade);
    }
}
