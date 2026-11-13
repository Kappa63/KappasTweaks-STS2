using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace KappasTweaks.KappasTweaksCode.Ancients;

internal static class Act1AncientPatch
{
    private static IEnumerable<AncientEventModel> WithKappa(IEnumerable<AncientEventModel> ancients)
    {
        return ancients.Append(ModelDb.AncientEvent<KappaTheAncient>());
    }

    [HarmonyPatch(typeof(Underdocks), nameof(Underdocks.GetUnlockedAncients))]
    internal static class UnderdocksPatch
    {
        [HarmonyPostfix]
        private static IEnumerable<AncientEventModel> AddKappa(IEnumerable<AncientEventModel> __result)
        {
            return WithKappa(__result);
        }
    }

    [HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.GetUnlockedAncients))]
    internal static class OvergrowthPatch
    {
        [HarmonyPostfix]
        private static IEnumerable<AncientEventModel> AddKappa(IEnumerable<AncientEventModel> __result)
        {
            return WithKappa(__result);
        }
    }

    [HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted")]
    internal static class RunStartRitualPatch
    {
        [HarmonyPrefix]
        private static void EmptyHpFirst(AncientEventModel __instance, bool isPreFinished)
        {
            if (isPreFinished || __instance is not KappasTweaksAncient || __instance.Owner is not { } owner)
                return;

            IRunState run = owner.RunState;
            if (run.CurrentActIndex != 0 || run.CurrentMapCoord != run.Map.StartingMapPoint.coord)
                return;

            owner.Creature.SetCurrentHpInternal(0m);
        }
    }
}
