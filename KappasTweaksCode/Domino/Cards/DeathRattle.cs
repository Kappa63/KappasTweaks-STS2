using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class DeathRattle() : BonesmanCard(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    // HYPER
    private const int _damage = 4;
    private const int _bonus = 3;
    private const int _bonusUpgrade = 1;

    protected override (int Left, int Right)? FixedPips => (2, 4);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(_damage),
        new ExtraDamageVar(_bonus),
        ScaledDamage(c => c.BreakCount),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await HitAllScaled(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(_bonusUpgrade);
    }
}
