using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class DominoEffect() : BonesmanCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 6;
    private const int _damageUpgrade = 2;
    private const int _perLink = 2;
    private const int _perLinkUpgrade = 1;

    protected override (int Left, int Right)? FixedPips => (3, 4);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(_damage),
        new ExtraDamageVar(_perLink),
        ScaledDamage(c => c.WillChain ? c.ChainCount : 0),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await HitScaled(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(_damageUpgrade);
        DynamicVars.ExtraDamage.UpgradeValueBy(_perLinkUpgrade);
    }
}
