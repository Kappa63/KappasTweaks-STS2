using System.Reflection;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

/// <summary>
/// Pair with [Ascension] to give that level a third boss in the given act. Act index is the same 0-based one ActModel
/// uses, so Act 1 is 0.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class TripleBossAtAttribute(int actIndex) : Attribute
{
    internal int ActIndex { get; } = actIndex;
}

internal static class TripleBoss
{
    private const int _actCount = 3;

    private static readonly (int Act, AscensionLevel Level)[] _stages = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Select(t => (At: t.GetCustomAttribute<TripleBossAtAttribute>(), Asc: t.GetCustomAttribute<AscensionAttribute>()))
        .Where(t => t is { At: not null, Asc: not null })
        .Select(t => (t.At!.ActIndex, (AscensionLevel)t.Asc!.Level))
        .ToArray();

    // One slot per act. Stored, not derived: picking the leftover boss out of the pool would repick whenever the pool
    // changes, so a run saved before a new boss is added would come back with a different third boss, and two clients
    // on different mod versions would disagree. The entry string rather than a ModelId because PacketWriter only
    // exposes WriteString.
    private static readonly SavedSpireField<IRunState, string?>[] _entries = Enumerable.Range(0, _actCount)
        .Select(act => new SavedSpireField<IRunState, string?>(() => null, $"ThirdBossEntry{act}")
        {
            Serializer = (entry, writer) => writer.WriteString(entry ?? string.Empty),
            Deserializer = reader => reader.ReadString(),
        })
        .ToArray();

    // RunManager.State is private, so the run state is taken from the hook that hands it over publicly.
    internal static IRunState? Run { get; set; }

    internal static bool IsActive(ActModel act)
    {
        return act.HasSecondBoss && IsAscensionActive(act);
    }

    /// <summary>
    /// Without the second boss check. Asset preloading happens before the second boss is assigned, so gating the
    /// preload on IsActive is what left the third boss's art uncached -- and feeding uncached art to Spine is fatal.
    /// </summary>
    internal static bool IsAscensionActive(ActModel act)
    {
        return _stages.Any(s => s.Act == act.Index && AscensionHelper.HasAscension(s.Level));
    }

    internal static EncounterModel? ThirdBoss(ActModel act)
    {
        if (Run is not { } run || Entry(act.Index) is not { } field || field[run] is not { Length: > 0 } entry)
            return null;

        return act.AllBossEncounters.FirstOrDefault(e => e.Id.Entry == entry);
    }

    // Seeded off the run so every client lands on the same boss without a message, then persisted so the answer
    // outlives any later change to the pool.
    internal static void RollIfNeeded(IRunState run)
    {
        ActModel act = run.Act;
        if (!IsActive(act) || Entry(act.Index) is not { } field || field[run] is { Length: > 0 })
            return;

        EncounterModel[] candidates = act.AllBossEncounters
            .Where(e => e.Id.Entry != act.BossEncounter.Id.Entry && e.Id.Entry != act.SecondBossEncounter?.Id.Entry)
            .OrderBy(e => e.Id.Entry, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
            return;

        field[run] = new Rng(run.Rng.Seed, $"act_{act.Index + 1}_third_boss").NextItem(candidates).Id.Entry;
    }

    private static SavedSpireField<IRunState, string?>? Entry(int actIndex)
    {
        return actIndex >= 0 && actIndex < _entries.Length ? _entries[actIndex] : null;
    }
}

// Vanilla preloads art for the boss and, if one is already assigned, the second. Both Double Boss and Triple Boss
// hand an act an extra boss that may not be picked yet when this runs, and a boss node whose art is missing at draw
// time takes the process down inside the Spine binding -- no exception, nothing catchable. Preloading every boss in
// the act is a few hundred KB and removes that whole class of crash, so it is not worth gating on which ascension
// is live.
[HarmonyPatch(typeof(ActModel), nameof(ActModel.AssetPaths), MethodType.Getter)]
internal static class TripleBossAssetPatch
{
    [HarmonyPostfix]
    private static void PreloadEveryBossNode(ActModel __instance, ref IEnumerable<string> __result)
    {
        __result = __result.Concat(__instance.AllBossEncounters.SelectMany(e => e.MapNodeAssetPaths)).Distinct();
    }
}

// MapNodeAssetPaths exists to list paths for the preloader, but it picks its branch by constructing
// BossNodeSpineResource, which calls Cache.GetAsset -- so merely asking which paths to preload synchronously loads
// any boss skeleton that is not cached yet, off the main thread, and files it under _missedCacheAssets. The next
// room load calls UnloadMissedCacheAssets, which disposes it while a boss map node is still drawing it: a native
// crash, no exception. Vanilla walks into this on its own boss the first time an act is entered; listing every boss
// widened it. ResourceLoader.Exists is the same test the getter starts with, without the load.
[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.MapNodeAssetPaths), MethodType.Getter)]
internal static class TripleBossNodePathPatch
{
    [HarmonyPrefix]
    private static bool WithoutLoadingAnything(EncounterModel __instance, ref IEnumerable<string> __result)
    {
        string path = __instance.BossNodePath;
        if (ResourceLoader.Exists(path))
        {
            __result = [path];
            return false;
        }

        // A path that resolves to nothing still reaches the loader, and Godot's resource parser dies natively on a
        // missing file rather than returning null -- the hazard AssetCache.MarkAssetFailed exists to contain. Vanilla
        // never listed a boss whose art it had not already drawn, so it never emitted one of these.
        string[] fallback = [$"{path}.png", $"{path}_outline.png"];
        __result = fallback.Where(p => ResourceLoader.Exists(p)).ToArray();

        foreach (string missing in fallback.Where(p => !ResourceLoader.Exists(p)))
            MainFile.Logger.Info($"Boss node art missing, not preloading: {missing}");

        return false;
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
internal static class TripleBossActPatch
{
    [HarmonyPostfix]
    private static void RollThirdBoss(IRunState runState)
    {
        TripleBoss.Run = runState;
        TripleBoss.RollIfNeeded(runState);
    }
}

// RoomSet.NextBossEncounter only knows how to alternate between two, so the third is chosen here. The current room is
// not in the history yet -- CreateRoom runs before AppendToMapPointHistory -- so two prior bosses means this is third.
[HarmonyPatch(typeof(ActModel), nameof(ActModel.PullNextEncounter))]
internal static class TripleBossEncounterPatch
{
    [HarmonyPostfix]
    private static void UseThirdBoss(ActModel __instance, RoomType roomType, ref EncounterModel __result)
    {
        if (roomType != RoomType.Boss || !TripleBoss.IsActive(__instance))
            return;

        if (TripleBoss.Run is not { } run || TripleBoss.ThirdBoss(__instance) is not { } third)
            return;

        int defeated = run.MapPointHistory[run.CurrentActIndex]
            .SelectMany(point => point.Rooms)
            .Count(room => room.RoomType == RoomType.Boss);

        if (defeated >= 2)
            __result = third;
    }
}
