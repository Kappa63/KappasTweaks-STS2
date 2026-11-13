using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
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

    internal static bool AtThird(IRunState? run)
    {
        return TripleBossMap.Third(run?.Map) is { } third && run!.CurrentMapCoord == third.coord;
    }
}

// Vanilla sends the player back to the map after the first of two bosses and moves to the next act after the second,
// recognising only SecondBossMapPoint ?? BossMapPoint as the last boss. With three, the second takes the first's path
// and the third takes the act-change path vanilla would have given the second.
[HarmonyPatch(typeof(NRewardsScreen), "OnProceedButtonPressed")]
internal static class TripleBossProceedPatch
{
    [HarmonyPrefix]
    private static bool ProceedPastBoss(NRewardsScreen __instance)
    {
        if (!_isTerminal(__instance))
            return true;

        IRunState run = _runState(__instance);
        if (run.CurrentRoom is not { } room || (room.RoomType != RoomType.Boss && !room.IsVictoryRoom))
            return true;

        if (TripleBossFlow.AtSecondOfThree(run))
        {
            TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreenAsync());
            return false;
        }

        if (!TripleBossFlow.AtThird(run))
            return true;

        _proceedButton(__instance).Disable();
        _disableProceedForever(__instance) = true;
        if (RunManager.Instance.ActChangeSynchronizer.IsWaitingForOtherPlayers())
            _waitingOverlay(__instance).Visible = true;

        RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
        return false;
    }

    private static readonly AccessTools.FieldRef<NRewardsScreen, bool> _isTerminal =
        AccessTools.FieldRefAccess<NRewardsScreen, bool>("_isTerminal");

    private static readonly AccessTools.FieldRef<NRewardsScreen, IRunState> _runState =
        AccessTools.FieldRefAccess<NRewardsScreen, IRunState>("_runState");

    private static readonly AccessTools.FieldRef<NRewardsScreen, NProceedButton> _proceedButton =
        AccessTools.FieldRefAccess<NRewardsScreen, NProceedButton>("_proceedButton");

    private static readonly AccessTools.FieldRef<NRewardsScreen, bool> _disableProceedForever =
        AccessTools.FieldRefAccess<NRewardsScreen, bool>("_disableProceedForever");

    private static readonly AccessTools.FieldRef<NRewardsScreen, Control> _waitingOverlay =
        AccessTools.FieldRefAccess<NRewardsScreen, Control>("_waitingForOtherPlayersOverlay");
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
