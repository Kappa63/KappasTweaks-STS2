using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Bonecrusher() : BonesmanCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 16;
    private const int _damageUpgrade = 5;
    private const int _vulnerable = 2;

    protected override (int Left, int Right)? FixedPips => (5, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new PowerVar<VulnerablePower>(_vulnerable),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue);
        if (chained)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target!, DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
