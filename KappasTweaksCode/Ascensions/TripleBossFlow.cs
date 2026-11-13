using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

internal static class TripleBossFlow
{
    /// <summary>True while the player is standing on the second boss with a third still ahead of them.</summary>
    internal static bool AtSecondOfThree(IRunState? run)
    {
        if (run?.Map is not { } map || map.SecondBossMapPoint is not { } second)
            return false;

        return TripleBossMap.Third(map) != null && run.CurrentMapCoord == second.coord;
    }
}

// Vanilla sends the player back to the map after the first of two bosses and ends the run after the second. With a
// third to come, the second boss has to take the same path the first one does.
[HarmonyPatch(typeof(NRewardsScreen), "OnProceedButtonPressed")]
internal static class TripleBossProceedPatch
{
    [HarmonyPrefix]
    private static bool ProceedToThirdBoss(NRewardsScreen __instance)
    {
        if (!AccessTools.FieldRefAccess<NRewardsScreen, bool>(__instance, "_isTerminal"))
            return true;

        IRunState run = AccessTools.FieldRefAccess<NRewardsScreen, IRunState>(__instance, "_runState");
        if (run.CurrentRoom is not { } room || (room.RoomType != RoomType.Boss && !room.IsVictoryRoom))
            return true;

        if (!TripleBossFlow.AtSecondOfThree(run))
            return true;

        TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreenAsync());
        return false;
    }
}

// The run-won stamp is keyed to the second boss being the last one, so hold it back until the third is down. Guarded
// narrowly on the second boss tile, since this setter also runs while loading an already-won run.
[HarmonyPatch(typeof(RunManager), nameof(RunManager.WinTime), MethodType.Setter)]
internal static class TripleBossWinTimePatch
{
    [HarmonyPrefix]
    private static bool HoldUntilThirdBoss(long value)
    {
        return value <= 0 || !TripleBossFlow.AtSecondOfThree(TripleBoss.Run);
    }
}
