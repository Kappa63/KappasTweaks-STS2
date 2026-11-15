using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class HotStreak() : BonesmanCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 6;
    private const int _damageUpgrade = 2;
    private const int _bonus = 2;
    private const int _bonusUpgrade = 1;

    protected override (int Left, int Right)? FixedPips => (4, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(_damage),
        new ExtraDamageVar(_bonus),
        ScaledDamage(c => c.ChainedThisTurn),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await HitScaled(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(_damageUpgrade);
        DynamicVars.ExtraDamage.UpgradeValueBy(_bonusUpgrade);
    }
}
