using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

/// <summary>
/// The third boss node. ActMap has no field for one, so it hangs off a side table keyed by map instance, and the
/// lookups that special-case the first two boss points are extended to answer for it.
/// </summary>
internal static class TripleBossMap
{
    // The first boss is left exactly where the game puts it -- that spot is on the paper by definition -- and the
    // other two are placed from it, same height, stepping right into open paper. No absolute coordinates: every y I
    // have invented so far has ended up off the top of the map.
    private const float _stepX = 350f;

    private static readonly ConditionalWeakTable<ActMap, MapPoint> _points = new();

    internal static MapPoint? Third(ActMap? map)
    {
        return map != null && _points.TryGetValue(map, out MapPoint? point) ? point : null;
    }

    internal static void Attach(IRunState run, ActMap map)
    {
        if (map.SecondBossMapPoint is not { } second || Third(map) != null)
            return;

        TripleBoss.Run = run;
        TripleBoss.RollIfNeeded(run);
        if (!TripleBoss.IsActive(run.Act) || TripleBoss.ThirdBoss(run.Act) == null)
            return;

        MapPoint third = new(second.coord.col, second.coord.row + 1)
        {
            PointType = MapPointType.Boss,
            CanBeModified = false,
        };

        second.AddChildPoint(third);
        _points.Add(map, third);
    }

    /// <summary>
    /// Adds the node for the third boss, leaving every node the game placed exactly where it put it. Must run before
    /// anything can look it up: RecalculateTravelability reaches it through MapTravel.GetTravelablePointsFrom and
    /// indexes the dictionary unguarded, so a missing node is a hard crash.
    /// </summary>
    internal static void EnsureNode(NMapScreen screen)
    {
        try
        {
            ActMap? map = _mapField(screen);
            RunState run = _runStateField(screen);
            if (Third(map) is not { } third || map!.SecondBossMapPoint is not { } second
                || TripleBoss.ThirdBoss(run.Act) is not { } encounter)
                return;

            Dictionary<MapCoord, NMapPoint> lookup = _dictField(screen);
            if (lookup.ContainsKey(third.coord) || !lookup.TryGetValue(second.coord, out NMapPoint? secondNode))
                return;

            NBossMapPoint node = NBossMapPoint.Create(third, screen, encounter, run);

            // Position and scale before the node enters the tree, the way SetMap does it for every boss node.
            // Setting them afterwards is what parked nodes in the top-left corner.
            node.Position = secondNode.Position + new Vector2(_stepX, 0f);
            node.Scale = secondNode.Scale;

            _containerField(screen).AddChild(node);

            lookup[third.coord] = node;

            // Only safe once the node is in the dictionary; DrawPaths throws on an unknown child.
            _drawPaths(screen, secondNode, second);

            node.FocusNeighborTop = node.GetPath();
            node.FocusNeighborRight = node.GetPath();
            node.FocusNeighborBottom = node.GetPath();
            node.FocusNeighborLeft = secondNode.GetPath();

            MainFile.Logger.Info($"Third boss node placed at {node.Position}, beside {secondNode.Position}");
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Could not build the third boss node: {e}");
        }
    }

    /// <summary>
    /// Puts the three bosses in a row. Runs only after SetMap has finished, because it rebuilds the paths SetMap
    /// drew from the old positions.
    /// </summary>
    internal static void ApplyLayout(NMapScreen screen)
    {
        try
        {
            ActMap? map = _mapField(screen);
            if (Third(map) is not { } third || map!.SecondBossMapPoint is not { } second)
                return;

            Dictionary<MapCoord, NMapPoint> lookup = _dictField(screen);
            if (!lookup.TryGetValue(map.BossMapPoint.coord, out NMapPoint? firstNode)
                || !lookup.TryGetValue(second.coord, out NMapPoint? secondNode)
                || !lookup.TryGetValue(third.coord, out NMapPoint? thirdNode))
            {
                return;
            }

            // Read, never invented. SetMap has just written the first boss's position, so this is the game's own
            // value and the two that follow inherit its height.
            Vector2 anchor = firstNode.Position;
            firstNode.Position = anchor - new Vector2(_stepX, 0f);
            secondNode.Position = anchor + new Vector2(0, 0f);
            thirdNode.Position = anchor + new Vector2(_stepX, 0f);
            thirdNode.Scale = secondNode.Scale;

            Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>> paths = _pathsField(screen);

            // Every point in the last row climbs to the first boss, which has just moved, so those converging paths
            // still end where it used to be and have to be laid again.
            foreach (MapPoint approach in map.GetPointsInRow(map.GetRowCount() - 1))
            {
                if (!lookup.TryGetValue(approach.coord, out NMapPoint? approachNode))
                    continue;

                DiscardPath(paths, approach.coord, map.BossMapPoint.coord);
                _drawPaths(screen, approachNode, approach);
            }

            DiscardPath(paths, map.BossMapPoint.coord, second.coord);
            DiscardPath(paths, second.coord, third.coord);
            _drawPaths(screen, firstNode, map.BossMapPoint);
            _drawPaths(screen, secondNode, second);

            firstNode.FocusNeighborTop = firstNode.GetPath();
            firstNode.FocusNeighborRight = secondNode.GetPath();
            secondNode.FocusNeighborTop = secondNode.GetPath();
            secondNode.FocusNeighborLeft = firstNode.GetPath();
            secondNode.FocusNeighborRight = thirdNode.GetPath();
            thirdNode.FocusNeighborTop = thirdNode.GetPath();
            thirdNode.FocusNeighborBottom = thirdNode.GetPath();
            thirdNode.FocusNeighborLeft = secondNode.GetPath();
            thirdNode.FocusNeighborRight = thirdNode.GetPath();

            MainFile.Logger.Info(
                $"Boss row at {firstNode.Position}, {secondNode.Position}, {thirdNode.Position}");
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Could not lay out the boss row: {e}");
        }
    }

    private static void DiscardPath(
        Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>> paths,
        MapCoord from,
        MapCoord to)
    {
        if (!paths.Remove((from, to), out IReadOnlyList<TextureRect>? stale))
            return;

        foreach (TextureRect tick in stale)
            tick.QueueFree();
    }

    private static readonly AccessTools.FieldRef<NMapScreen, Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>>> _pathsField =
        AccessTools.FieldRefAccess<NMapScreen, Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>>>("_paths");

    private static readonly AccessTools.FieldRef<NMapScreen, ActMap> _mapField =
        AccessTools.FieldRefAccess<NMapScreen, ActMap>("_map");

    private static readonly AccessTools.FieldRef<NMapScreen, RunState> _runStateField =
        AccessTools.FieldRefAccess<NMapScreen, RunState>("_runState");

    private static readonly AccessTools.FieldRef<NMapScreen, Control> _containerField =
        AccessTools.FieldRefAccess<NMapScreen, Control>("_points");

    private static readonly AccessTools.FieldRef<NMapScreen, Dictionary<MapCoord, NMapPoint>> _dictField =
        AccessTools.FieldRefAccess<NMapScreen, Dictionary<MapCoord, NMapPoint>>("_mapPointDictionary");

    private static readonly Action<NMapScreen, NMapPoint, MapPoint> _drawPaths =
        AccessTools.MethodDelegate<Action<NMapScreen, NMapPoint, MapPoint>>(
            AccessTools.Method(typeof(NMapScreen), "DrawPaths"));
}

[HarmonyPatch(typeof(RunState), nameof(RunState.Map), MethodType.Setter)]
internal static class TripleBossMapAttachPatch
{
    [HarmonyPostfix]
    private static void AttachThirdPoint(RunState __instance, ActMap value)
    {
        TripleBossMap.Attach(__instance, value);
    }
}

// ActMap answers for the boss and starting points by hand because they live outside the grid. The third needs the
// same treatment or travelling to it resolves to nothing.
[HarmonyPatch(typeof(ActMap), nameof(ActMap.GetPoint), typeof(int), typeof(int))]
internal static class TripleBossGetPointPatch
{
    [HarmonyPostfix]
    private static void IncludeThirdPoint(ActMap __instance, int col, int row, ref MapPoint? __result)
    {
        if (__result == null && TripleBossMap.Third(__instance) is { } third
            && third.coord.col == col && third.coord.row == row)
        {
            __result = third;
        }
    }
}

[HarmonyPatch(typeof(ActMap), nameof(ActMap.HasPoint))]
internal static class TripleBossHasPointPatch
{
    [HarmonyPostfix]
    private static void IncludeThirdPoint(ActMap __instance, MapCoord coord, ref bool __result)
    {
        if (!__result && TripleBossMap.Third(__instance) is { } third && third.coord == coord)
            __result = true;
    }
}

[HarmonyPatch(typeof(NMapScreen), "RecalculateTravelability")]
internal static class TripleBossTravelabilityPatch
{
    [HarmonyPrefix]
    private static void BuildThirdNodeFirst(NMapScreen __instance)
    {
        TripleBossMap.EnsureNode(__instance);
    }
}

// Covers the case where the map is built while hidden, so RecalculateTravelability never runs.
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
internal static class TripleBossSetMapPatch
{
    [HarmonyPostfix]
    private static void BuildThirdNode(NMapScreen __instance)
    {
        TripleBossMap.EnsureNode(__instance);
        TripleBossMap.ApplyLayout(__instance);
    }
}
