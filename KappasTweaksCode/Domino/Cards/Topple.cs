using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Topple() : BonesmanCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    // HYPER
    private const int _damage = 6;
    private const int _damageUpgrade = 2;
    private const int _vulnerable = 1;

    protected override (int Left, int Right)? FixedPips => (4, 5);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new PowerVar<VulnerablePower>(_vulnerable),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await HitAll(choiceContext, play, DynamicVars.Damage.BaseValue);
        await HitAll(choiceContext, play, DynamicVars.Damage.BaseValue);
        if (chained)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState!.HittableEnemies, DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
