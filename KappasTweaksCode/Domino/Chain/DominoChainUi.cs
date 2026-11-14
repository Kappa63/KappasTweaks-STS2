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
    private const float _pip = 14f;
    private const float _border = 4f;
    private static readonly Color _bone = new("EFE6CF");
    private static readonly Color _ink = new("2A2420");

    private const string _nodeName = "BonesmanDomino";
    private static Control? _root;
    private static Player? _me;
    private static readonly List<Control>[] _halves = [[], []];

    static DominoChainUi()
    {
        ChainPower.Changed += player =>
        {
            if (player == _me)
                Refresh(pulse: true);
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
        Control root = new()
        {
            Name = _nodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorTop = 1f,
            AnchorBottom = 1f,
            OffsetLeft = _left,
            OffsetRight = _left + _width,
            OffsetTop = _bottom - _width * 2,
            OffsetBottom = _bottom,
            PivotOffset = new Vector2(_width / 2, _width),
        };

        Panel tile = new() { MouseFilter = Control.MouseFilterEnum.Ignore };
        tile.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        StyleBoxFlat face = Box(_bone, radius: 14);
        face.BorderColor = _ink;
        face.SetBorderWidthAll((int)_border);
        face.ShadowColor = new Color(0, 0, 0, 0.45f);
        face.ShadowSize = 6;
        tile.AddThemeStyleboxOverride("panel", face);
        root.AddChild(tile);

        ColorRect divider = new()
        {
            Color = _ink,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(_width * 0.15f, _width - _border / 2),
            Size = new Vector2(_width * 0.7f, _border),
        };
        root.AddChild(divider);

        for (int half = 0; half < 2; half++)
        {
            _halves[half].Clear();
            foreach (Vector2 spot in _spots)
            {
                Panel pip = new()
                {
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    Size = new Vector2(_pip, _pip),
                    Position = spot * _width - new Vector2(_pip / 2, _pip / 2) + new Vector2(0, half * _width),
                };
                pip.AddThemeStyleboxOverride("panel", Box(_ink, radius: (int)(_pip / 2)));
                root.AddChild(pip);
                _halves[half].Add(pip);
            }
        }
        return root;
    }

    private static void Refresh(bool pulse)
    {
        if (_root == null || !GodotObject.IsInstanceValid(_root))
            return;

        ChainPower? chain = Chain.Of(_me);
        bool active = chain is { HasEnds: true };
        _root.Modulate = active ? Colors.White : new Color(1, 1, 1, 0.45f);
        Show(0, active ? chain!.Left : ChainPower.Blank);
        Show(1, active ? chain!.Right : ChainPower.Blank);

        if (!pulse)
            return;
        _root.Scale = Vector2.One * 1.15f;
        _root.CreateTween().TweenProperty(_root, "scale", Vector2.One, 0.25)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
    }

    private static void Show(int half, int pips)
    {
        for (int i = 0; i < _spots.Length; i++)
            _halves[half][i].Visible = _faces[pips].Contains(i);
    }

    private static StyleBoxFlat Box(Color color, int radius)
    {
        StyleBoxFlat box = new() { BgColor = color };
        box.SetCornerRadiusAll(radius);
        return box;
    }

    // Spots inside one square half, as fractions of its size: TL, TR, ML, C, MR, BL, BR.
    private static readonly Vector2[] _spots =
    [
        new(0.27f, 0.27f), new(0.73f, 0.27f), new(0.27f, 0.5f), new(0.5f, 0.5f),
        new(0.73f, 0.5f), new(0.27f, 0.73f), new(0.73f, 0.73f),
    ];

    private static readonly int[][] _faces =
    [
        [], [3], [0, 6], [0, 3, 6], [0, 1, 5, 6], [0, 1, 3, 5, 6], [0, 1, 2, 4, 5, 6],
    ];
}
