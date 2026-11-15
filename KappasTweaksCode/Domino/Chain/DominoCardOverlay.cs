using Godot;
using HarmonyLib;
using KappasTweaks.KappasTweaksCode.Domino.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Draws each domino card's pips as a tile on its art, so a hand can be read at a glance. The half that would Chain
// lights up gold.
[HarmonyPatch(typeof(NCard))]
internal static class DominoCardOverlay
{
    // HYPER
    private const float _half = 40f;
    // Top-left corner of the art, the part overlapping cards in hand leave visible.
    private static readonly Vector2 _position = new(-104f, -154f);

    private const string _nodeName = "BonesmanPips";
    private static readonly List<WeakReference<NCard>> _shown = [];

    static DominoCardOverlay()
    {
        ChainPower.Changed += _ => RefreshAll();
        BonesmanCard.PipsChanged += card => RefreshAll(card);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Reload")]
    private static void AfterReload(NCard __instance) => Sync(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCard.UpdateVisuals))]
    private static void AfterUpdateVisuals(NCard __instance) => Sync(__instance);

    // Purely cosmetic, and it runs inside card rendering and combat hooks: it must never throw into the game.
    private static void Sync(NCard card)
    {
        try
        {
            SyncUnsafe(card);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Domino tile failed for {card.Model?.Id}: {e}");
        }
    }

    private static void SyncUnsafe(NCard card)
    {
        if (!GodotObject.IsInstanceValid(card) || card.Body is not { } body)
            return;

        Control? tile = body.GetNodeOrNull<Control>(_nodeName);
        if (card.Model is not BonesmanCard { Domino: true } domino)
        {
            tile?.QueueFree();
            return;
        }

        if (tile == null)
        {
            tile = DominoTile.Create(_half, vertical: true, _nodeName);
            tile.Position = _position;
            body.AddChild(tile);
            _shown.Add(new WeakReference<NCard>(card));
        }
        DominoTile.Set(tile, domino.PipLeft, domino.PipRight, domino.GlowHalf);
    }

    private static void RefreshAll(BonesmanCard? only = null)
    {
        _shown.RemoveAll(r => !r.TryGetTarget(out NCard? card) || !GodotObject.IsInstanceValid(card));
        foreach (WeakReference<NCard> reference in _shown.ToList())
        {
            if (reference.TryGetTarget(out NCard? card) && (only == null || card.Model == only))
                Sync(card);
        }
    }
}
