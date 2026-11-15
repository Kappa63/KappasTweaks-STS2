using System.Runtime.CompilerServices;
using Godot;

namespace KappasTweaks.KappasTweaksCode.Domino;

// A domino drawn from plain panels (no registered Godot scripts). `size` is one half's edge length; horizontal tiles
// put the left pip on the left, vertical ones put it on top.
internal static class DominoTile
{
    private static readonly Color _bone = new("EFE6CF");
    private static readonly Color _gold = new("F2C94C");
    private static readonly Color _ink = new("2A2420");
    private static readonly Color _wild = new("7A3FB0");
    private static readonly ConditionalWeakTable<Control, Parts> _parts = new();

    private sealed class Parts
    {
        public readonly Panel[] Halves = new Panel[2];
        public readonly List<Control>[] Pips = [[], []];
        public readonly Label[] Unknown = new Label[2];
        public readonly Polygon2D[] Stars = new Polygon2D[2];
    }

    public static Control Create(float size, bool vertical, string name)
    {
        float border = Mathf.Max(2f, size / 20f);
        Vector2 full = vertical ? new Vector2(size, size * 2) : new Vector2(size * 2, size);
        Control root = new() { Name = name, MouseFilter = Control.MouseFilterEnum.Ignore, Size = full, PivotOffset = full / 2 };
        Parts parts = new();

        Panel shadow = new() { MouseFilter = Control.MouseFilterEnum.Ignore, Size = full, Position = new Vector2(border, border * 1.5f) };
        shadow.AddThemeStyleboxOverride("panel", Box(new Color(0, 0, 0, 0.45f), size / 6));
        root.AddChild(shadow);

        for (int half = 0; half < 2; half++)
        {
            Vector2 offset = vertical ? new Vector2(0, half * size) : new Vector2(half * size, 0);
            Panel face = new() { MouseFilter = Control.MouseFilterEnum.Ignore, Size = new Vector2(size, size), Position = offset };
            root.AddChild(face);
            parts.Halves[half] = face;

            foreach (Vector2 spot in _spots)
            {
                float pip = size * 0.17f;
                Panel dot = new()
                {
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    Size = new Vector2(pip, pip),
                    Position = offset + spot * size - new Vector2(pip / 2, pip / 2),
                };
                dot.AddThemeStyleboxOverride("panel", Box(_ink, pip / 2));
                root.AddChild(dot);
                parts.Pips[half].Add(dot);
            }

            Label unknown = new()
            {
                Text = "?",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Size = new Vector2(size, size),
                Position = offset,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            unknown.AddThemeColorOverride("font_color", _ink);
            unknown.AddThemeFontSizeOverride("font_size", (int)(size * 0.6f));
            root.AddChild(unknown);
            parts.Unknown[half] = unknown;

            Polygon2D star = new() { Color = _wild, Position = offset + new Vector2(size / 2, size / 2), Polygon = Star(size * 0.4f) };
            root.AddChild(star);
            parts.Stars[half] = star;
        }

        ColorRect divider = new()
        {
            Color = _ink,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = vertical ? new Vector2(size * 0.12f, size - border / 2) : new Vector2(size - border / 2, size * 0.12f),
            Size = vertical ? new Vector2(size * 0.76f, border) : new Vector2(border, size * 0.76f),
        };
        root.AddChild(divider);

        Panel outline = new() { MouseFilter = Control.MouseFilterEnum.Ignore, Size = full };
        StyleBoxFlat line = Box(Colors.Transparent, size / 6);
        line.BorderColor = _ink;
        line.SetBorderWidthAll((int)border);
        outline.AddThemeStyleboxOverride("panel", line);
        root.AddChild(outline);

        _parts.Add(root, parts);
        Set(root, -1, -1);
        return root;
    }

    // -1 shows "?". `glow` (0 = left/top, 1 = right/bottom) tints the half that would Chain.
    public static void Set(Control tile, int left, int right, int glow = -1)
    {
        if (!_parts.TryGetValue(tile, out Parts? parts))
            return;
        int[] values = [left, right];
        for (int half = 0; half < 2; half++)
        {
            float size = parts.Halves[half].Size.X;
            StyleBoxFlat face = Box(half == glow ? _gold : _bone, size / 6);
            face.CornerRadiusTopLeft = face.CornerRadiusBottomLeft = face.CornerRadiusTopRight = face.CornerRadiusBottomRight = 0;
            bool first = half == 0;
            bool vertical = parts.Halves[1].Position.Y > 0;
            int r = (int)(size / 6);
            if (vertical) { if (first) face.CornerRadiusTopLeft = face.CornerRadiusTopRight = r; else face.CornerRadiusBottomLeft = face.CornerRadiusBottomRight = r; }
            else { if (first) face.CornerRadiusTopLeft = face.CornerRadiusBottomLeft = r; else face.CornerRadiusTopRight = face.CornerRadiusBottomRight = r; }
            parts.Halves[half].AddThemeStyleboxOverride("panel", face);

            int value = values[half];
            parts.Unknown[half].Visible = value < 0;
            parts.Stars[half].Visible = value == ChainPower.Wild;
            for (int i = 0; i < _spots.Length; i++)
                parts.Pips[half][i].Visible = value is >= 0 and <= ChainPower.MaxPip && _faces[value].Contains(i);
        }
    }

    private static Vector2[] Star(float radius)
    {
        Vector2[] points = new Vector2[10];
        for (int i = 0; i < 10; i++)
            points[i] = Vector2.FromAngle(-Mathf.Pi / 2 + i * Mathf.Pi / 5) * (i % 2 == 0 ? radius : radius * 0.45f);
        return points;
    }

    private static StyleBoxFlat Box(Color color, float radius)
    {
        StyleBoxFlat box = new() { BgColor = color };
        box.SetCornerRadiusAll((int)radius);
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
