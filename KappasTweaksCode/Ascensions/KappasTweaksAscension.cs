using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

/// <summary>
/// Mark an ascension class with this and the cap patches below pick it up. Adding a new ascension means one new
/// file -- nothing in here changes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class AscensionAttribute(int level) : Attribute
{
    internal int Level { get; } = level;
}

internal static class KappasTweaksAscension
{
    // The game hardcodes 10 everywhere; every patch below only ever raises that ceiling to MaxLevel.
    private const int _vanillaMaxLevel = 10;

    internal static int MaxLevel { get; } = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Select(t => t.GetCustomAttribute<AscensionAttribute>())
        .OfType<AscensionAttribute>()
        .Select(a => a.Level)
        .Append(_vanillaMaxLevel)
        .Max();
}

[HarmonyPatch(typeof(ProgressSaveManager), "IncrementSingleplayerAscension")]
internal static class AscensionUnlockPatch
{
    [HarmonyPostfix]
    private static void UnlockNext(SerializableRun run, CharacterStats charStats)
    {
        if (run.Ascension != charStats.MaxAscension || charStats.MaxAscension >= KappasTweaksAscension.MaxLevel)
            return;

        charStats.MaxAscension++;
        charStats.PreferredAscension = charStats.MaxAscension;
    }
}

[HarmonyPatch(typeof(ProgressSaveManager), "IncrementMultiplayerAscension")]
internal static class AscensionMultiplayerUnlockPatch
{
    [HarmonyPostfix]
    private static void UnlockNext(ProgressSaveManager __instance, SerializableRun run)
    {
        ProgressState progress = __instance.Progress;
        if (run.Ascension != progress.MaxMultiplayerAscension
            || progress.MaxMultiplayerAscension >= KappasTweaksAscension.MaxLevel)
        {
            return;
        }

        progress.MaxMultiplayerAscension++;
        progress.PreferredMultiplayerAscension = progress.MaxMultiplayerAscension;
    }
}

// Undo the save-load clamp only where it actually clamped, so 11 stays 11 when the cap is 12.
[HarmonyPatch(typeof(ProgressState), "ClampAscension")]
internal static class AscensionClampPatch
{
    [HarmonyPostfix]
    private static void KeepCustomLevels(int value, ref int __result)
    {
        if (value > __result)
            __result = Math.Min(value, KappasTweaksAscension.MaxLevel);
    }
}

[HarmonyPatch(typeof(ProgressState), "ClampCharacterStatsFields")]
internal static class AscensionCharacterClampPatch
{
    [HarmonyPrefix]
    private static void RememberAscensions(CharacterStats stats, out (int Max, int Preferred) __state)
    {
        __state = (stats.MaxAscension, stats.PreferredAscension);
    }

    [HarmonyPostfix]
    private static void KeepCustomLevels(CharacterStats stats, (int Max, int Preferred) __state)
    {
        if (__state.Max > stats.MaxAscension)
            stats.MaxAscension = Math.Min(__state.Max, KappasTweaksAscension.MaxLevel);

        if (__state.Preferred > stats.PreferredAscension)
            stats.PreferredAscension = Math.Min(__state.Preferred, KappasTweaksAscension.MaxLevel);
    }
}

[HarmonyPatch(typeof(UnlockConsoleCmd), "UnlockAscensions")]
internal static class AscensionUnlockCmdPatch
{
    [HarmonyPostfix]
    private static void UnlockCustomLevels()
    {
        ProgressState progress = SaveManager.Instance.Progress;
        progress.MaxMultiplayerAscension = KappasTweaksAscension.MaxLevel;

        foreach (CharacterModel character in ModelDb.AllCharacters)
            progress.GetOrCreateCharacterStats(character.Id).MaxAscension = KappasTweaksAscension.MaxLevel;
    }
}
