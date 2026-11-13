using System.Reflection;
using Godot;
using HarmonyLib;
using KappasTweaks.KappasTweaksCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Multiplayer;

/// <summary>
/// A custom entry in the slot the draw pile and settings icons use, next to a teammate's health bar. Each client
/// judges its own piles and broadcasts the result, so the icon you see belongs to the remote player.
/// </summary>
public abstract class KappasTweaksLocationIcon
{
    // NetScreenType crosses the wire as ceil(log2(14) + 1) = 5 bits, so 15..31 are free. Anything past that would
    // corrupt the rest of the packet, hence the Take.
    private const int _firstValue = 15;
    private const int _lastValue = 31;

    // Sorted so every peer gives the same class the same value, and so two active icons resolve the same way.
    private static readonly KappasTweaksLocationIcon[] _all = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(KappasTweaksLocationIcon)) && !t.IsAbstract)
        .OrderBy(t => t.FullName, StringComparer.Ordinal)
        .Take(_lastValue - _firstValue + 1)
        .Select(Create)
        .ToArray();

    private static readonly MethodInfo _getCurrentScreen =
        AccessTools.Method(typeof(ScreenStateTracker), "GetCurrentScreen");

    private static Player? _me;

    private Texture2D? _texture;

    internal static KappasTweaksLocationIcon? Current { get; private set; }

    internal NetScreenType ScreenType { get; private set; }

    protected string AssetName => GetType().Name.ToSnakeCase();

    internal Texture2D? Texture =>
        _texture ??= ResourceLoader.Load<Texture2D>($"{AssetName}.png".MultiplayerIconPath());

    /// <summary>True while this icon should be showing. Only ever asked about the local player.</summary>
    protected abstract bool IsActive(Player player);

    internal static void Initialize()
    {
        CombatManager.Instance.CombatSetUp += OnCombatSetUp;
        CombatManager.Instance.CombatEnded += OnCombatEnded;
    }

    internal static KappasTweaksLocationIcon? FromScreenType(NetScreenType screenType)
    {
        return _all.FirstOrDefault(i => i.ScreenType == screenType);
    }

    private static KappasTweaksLocationIcon Create(Type type, int index)
    {
        KappasTweaksLocationIcon icon = (KappasTweaksLocationIcon)Activator.CreateInstance(type)!;
        icon.ScreenType = (NetScreenType)(_firstValue + index);
        return icon;
    }

    // Piles only live as long as the combat, and they hold the handler rather than the other way round, so there
    // is nothing to unsubscribe -- dropping _me is what actually turns the icons off.
    private static void OnCombatSetUp(CombatState combat)
    {
        _me = LocalContext.GetMe(combat);
        if (_me?.PlayerCombatState is not { } state)
            return;

        foreach (CardPile pile in state.AllPiles)
            pile.ContentsChanged += Refresh;

        Refresh();
    }

    private static void OnCombatEnded(CombatRoom _)
    {
        _me = null;
        Refresh();
    }

    private static void Refresh()
    {
        KappasTweaksLocationIcon? active = _me is { } me ? _all.FirstOrDefault(i => i.IsActive(me)) : null;
        if (active == Current)
            return;

        Current = active;
        Resync();
    }

    // ScreenStateTracker owns the value that gets broadcast, so re-asking it is what publishes a change.
    private static void Resync()
    {
        if (NRun.Instance?.ScreenStateTracker is not { } tracker)
            return;

        RunManager.Instance.InputSynchronizer.SyncLocalScreen((NetScreenType)_getCurrentScreen.Invoke(tracker, null)!);
    }
}

// Room is the only state with no icon of its own, so a custom icon fills that gap without ever hiding the map,
// deck or draw pile icons a teammate would otherwise expect when they actually open something.
[HarmonyPatch(typeof(ScreenStateTracker), "GetCurrentScreen")]
internal static class LocationIconScreenPatch
{
    [HarmonyPostfix]
    private static void UseCustomIcon(ref NetScreenType __result)
    {
        if (__result == NetScreenType.Room && KappasTweaksLocationIcon.Current is { } icon)
            __result = icon.ScreenType;
    }
}

// Vanilla's switch throws on anything it does not know, which would take down any client that receives a custom
// value -- including one sent by a different mod. Swallow the whole custom range, not just our own slots.
[HarmonyPatch(typeof(NetScreenTypeExtensions), nameof(NetScreenTypeExtensions.GetLocationIcon))]
internal static class LocationIconTexturePatch
{
    [HarmonyPrefix]
    private static bool CustomIcon(NetScreenType screenType, ref Texture2D? __result)
    {
        if (screenType <= NetScreenType.RemotePlayerExpandedState)
            return true;

        __result = KappasTweaksLocationIcon.FromScreenType(screenType)?.Texture;
        return false;
    }
}
