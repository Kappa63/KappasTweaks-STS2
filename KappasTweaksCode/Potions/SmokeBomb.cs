using System.Runtime.CompilerServices;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Rooms;

namespace KappasTweaks.KappasTweaksCode.Potions;

[Pool(typeof(SharedPotionPool))]
public sealed class SmokeBomb : KappasTweaksPotion
{
    private const string _vfx = "vfx/vfx_smoke_puff";
    
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        ICombatState combatState = Owner.Creature.CombatState!;
        EncounterModel? encounter = combatState.Encounter;

        if (encounter == null || encounter.RoomType == RoomType.Boss)
            return;
        VfxCmd.PlayOnCreatureCenter(Owner.Creature, _vfx);

        SmokeBombRewardsPatch.SuppressRewardsFor(encounter);

        foreach (Creature enemy in combatState.Enemies.ToList())
            await CreatureCmd.Escape(enemy);
    }
}

[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.ShouldGiveRewards), MethodType.Getter)]
internal static class SmokeBombRewardsPatch
{
    private static readonly object _marker = new();

    private static readonly ConditionalWeakTable<EncounterModel, object> _smokeBombed = new();

    internal static void SuppressRewardsFor(EncounterModel encounter) => _smokeBombed.AddOrUpdate(encounter, _marker);

    [HarmonyPostfix]
    private static void ForceNoRewards(EncounterModel __instance, ref bool __result)
    {
        if (__result && _smokeBombed.TryGetValue(__instance, out _))
            __result = false;
    }
}
