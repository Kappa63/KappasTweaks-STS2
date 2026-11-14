using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class FaultLine() : BonesmanCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    // HYPER
    private const int _damage = 7;
    private const int _damageUpgrade = 3;
    private const int _weak = 1;

    protected override (int Left, int Right)? FixedPips => (2, 5);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new PowerVar<WeakPower>(_weak),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await HitAll(choiceContext, play, DynamicVars.Damage.BaseValue);
        if (chained)
            await PowerCmd.Apply<WeakPower>(choiceContext, CombatState!.HittableEnemies, DynamicVars[nameof(WeakPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
