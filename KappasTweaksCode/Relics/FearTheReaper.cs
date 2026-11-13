using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KappasTweaks.KappasTweaksCode.Relics;

[Pool(typeof(SharedRelicPool))]
public sealed class FearTheReaper : KappasTweaksRelic
{
    private const int _combats = 3;
    private const decimal _hp = 1m;
    private const string _vfx = "vfx/vfx_spooky_scream";
    private const string _combatsKey = "Combats";

    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new (_combatsKey, _combats)];

    private int _combatsRemaining;
    
    public override bool ShowCounter => true;

    public override int DisplayAmount => CombatsRemaining;

    [SavedProperty]
    public int CombatsRemaining
    {
        get => _combatsRemaining;
        set
        {
            AssertMutable();
            _combatsRemaining = value;
        }
    }

    public override Task AfterObtained()
    {
        CombatsRemaining = DynamicVars[_combatsKey].IntValue;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task BeforeCombatStart()
    {
        if (CombatsRemaining <= 0)
            return;

        Flash();

        IReadOnlyList<Creature> enemies = Owner.Creature.CombatState!.HittableEnemies;
        VfxCmd.PlayOnCreatureCenters(enemies, _vfx);
        foreach (Creature enemy in enemies)
            await CreatureCmd.SetCurrentHp(enemy, _hp);
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.Side != CombatSide.Enemy || CombatsRemaining <= 0)
            return;

        Flash();
        VfxCmd.PlayOnCreatureCenter(creature, _vfx);
        await CreatureCmd.SetCurrentHp(creature, _hp);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (CombatsRemaining > 0)
        {
            CombatsRemaining--;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }
}
