using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class LastTile() : BonesmanCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    // HYPER
    private const int _perLink = 6;
    private const int _perLinkUpgrade = 2;

    protected override (int Left, int Right)? FixedPips => (6, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(0),
        new ExtraDamageVar(_perLink),
        ScaledDamage(c => Math.Max(1, c.ChainCount)),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await HitScaled(choiceContext, play);
        await BreakChain(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(_perLinkUpgrade);
    }
}
