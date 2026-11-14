using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class BoneStorm() : BonesmanCard(2, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
{
    // HYPER
    private const int _damage = 3;
    private const int _damageUpgrade = 1;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        int pips = Owner.PlayerCombatState!.Hand.Cards.OfType<BonesmanCard>().Where(c => c != this && c.Domino && c.PipLeft >= 0).Sum(c => c.PipLeft + c.PipRight);
        if (pips > 0)
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingRandomOpponents(CombatState!).WithHitCount(pips).WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
