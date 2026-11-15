using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace KappasTweaks.KappasTweaksCode.Domino;

// The open ends drawn as a domino where the Fool keeps his bow: left side, above the energy orb. Built from plain
// panels so the mod needs no registered Godot scripts.
[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
internal static class DominoChainUi
{
    // HYPER
    private const float _width = 80f;
    private const float _left = 27f;
    private const float _bottom = -265f;

    private const string _nodeName = "BonesmanDomino";
    private static Control? _root;
    private static Player? _me;

    static DominoChainUi()
    {
        ChainPower.Changed += player =>
        {
            if (player != _me)
                return;
            try
            {
                Refresh(pulse: true);
            }
            catch (Exception e)
            {
                MainFile.Logger.Error($"Domino chain display failed: {e}");
            }
        };
    }

    [HarmonyPostfix]
    private static void Attach(NCombatUi __instance, CombatState state)
    {
        __instance.GetNodeOrNull(_nodeName)?.QueueFree();
        _me = LocalContext.GetMe(state);
        if (_me?.Character is not Bonesman)
        {
            _root = null;
            return;
        }

        _root = Build();
        __instance.AddChild(_root);
        Refresh(pulse: false);
    }

    private static Control Build()
    {
        Control root = DominoTile.Create(_width, vertical: true, _nodeName);
        root.AnchorTop = 1f;
        root.AnchorBottom = 1f;
        root.OffsetLeft = _left;
        root.OffsetRight = _left + _width;
        root.OffsetTop = _bottom - _width * 2;
        root.OffsetBottom = _bottom;
        return root;
    }

    private static void Refresh(bool pulse)
    {
        if (_root == null || !GodotObject.IsInstanceValid(_root))
            return;

        ChainPower? chain = Chain.Of(_me);
        bool active = chain is { HasEnds: true };
        _root.Modulate = active ? Colors.White : new Color(1, 1, 1, 0.45f);
        DominoTile.Set(_root, active ? chain!.Left : ChainPower.Blank, active ? chain!.Right : ChainPower.Blank);

        if (!pulse)
            return;
        _root.Scale = Vector2.One * 1.15f;
        _root.CreateTween().TweenProperty(_root, "scale", Vector2.One, 0.25)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
    }
}
