using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[Ascension(Level)]
internal static class EmptyChest
{
    // HYPER
    internal const int Level = 15;

    internal const AscensionLevel Ascension = (AscensionLevel)Level;
}

[HarmonyPatch(typeof(Hook), nameof(Hook.ShouldGenerateTreasure))]
internal static class EmptyChestPatch
{
    [HarmonyPostfix]
    private static void SkipFirstChest(IRunState runState, ref bool __result)
    {
        if (!__result || !AscensionHelper.HasAscension(EmptyChest.Ascension))
            return;

        __result = runState.MapPointHistory
            .SelectMany(act => act)
            .SelectMany(point => point.Rooms)
            .Count(room => room.RoomType == RoomType.Treasure) > 1;
    }
}
