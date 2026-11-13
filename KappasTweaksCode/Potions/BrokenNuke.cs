using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Potions;

[Pool(typeof(SharedPotionPool))]
public sealed class BrokenNuke : KappasTweaksPotion
{
    // HYPER
    private const decimal _procPercent = 50m;
    private const int _damage = 500;
    private const string _procKey = "ProcPercentage";

    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AllEnemies;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(_damage, ValueProp.Unpowered),
        new (_procKey, _procPercent),
    ];

    private decimal ProcChance => DynamicVars[_procKey].BaseValue / 100m;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        Creature player = Owner.Creature;

        IReadOnlyList<Creature> targets = player.CombatState!.HittableEnemies;

        foreach (Creature enemy in targets)
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(enemy));

        await Cmd.CustomScaledWait(0.2f, 0.3f);

        if ((decimal)Owner.RunState.Rng.Niche.NextDouble() >= ProcChance)
            return;

        DamageVar damage = DynamicVars.Damage;
        await CreatureCmd.Damage(choiceContext, targets, damage.BaseValue, damage.Props, player, null, null);
    }
}
