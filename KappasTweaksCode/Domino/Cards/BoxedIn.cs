using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class BoxedIn() : BonesmanCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 8;
    private const int _damageUpgrade = 3;
    private const int _debuff = 1;

    protected override (int Left, int Right)? FixedPips => (4, 1);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new PowerVar<WeakPower>(_debuff),
        new PowerVar<VulnerablePower>(_debuff),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue);
        if (!chained)
            return;
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target!, DynamicVars[nameof(WeakPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target!, DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
