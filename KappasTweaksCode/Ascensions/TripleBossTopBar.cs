using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

// Not a typo: 32 of the game's types sit under a lowercase "sts2" namespace, this one among them.
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

// The top bar previews the boss still to come, which it works out as "am I standing on the first of two". Standing on
// the second of three reads as neither, so the bar hides itself and keeps the first boss's icon.
internal static class TripleBossTopBar
{
    // Vanilla tucks the second boss into the corner of the main icon at (30, 22). The third hangs off the second with
    // the same offset rather than a doubled one, so the three stagger evenly whatever the icon size.
    private const string _nodeName = "KappasThirdBossIcon";
    private static readonly Vector2 _offset = new(30f, 0f);

    internal static void ShowThirdBoss(NTopBarBossIcon bar)
    {
        try
        {
            TextureRect main = bar.GetNode<TextureRect>("Icon");

            if (TripleBoss.Run is not { } run
                || run.Map.SecondBossMapPoint is not { } second
                || TripleBossMap.Third(run.Map) == null
                || TripleBoss.ThirdBoss(run.Act) is not { } encounter)
            {
                Badge(main)?.SetVisible(false);
                return;
            }

            string icon = ImageHelper.GetRoomIconPath(MapPointType.Boss, RoomType.Boss, encounter.Id);
            string outline = ImageHelper.GetRoomIconOutlinePath(MapPointType.Boss, RoomType.Boss, encounter.Id);

            // On the second boss the bar is previewing what comes next, which is now the third rather than nothing.
            if (run.CurrentMapPoint == second)
            {
                main.Texture = PreloadManager.Cache.GetTexture2D(icon);
                bar.GetNode<TextureRect>("Icon/Outline").Texture = PreloadManager.Cache.GetTexture2D(outline);
                Badge(main)?.SetVisible(false);
                bar.Visible = true;
                return;
            }

            // Vanilla's second badge is the only other TextureRect hanging off the icon; chaining onto it keeps the
            // stagger even. If it has not been built yet there is nothing to sit beside, so wait for the next refresh.
            if (SecondBadge(main) is not { } secondBadge)
                return;

            TextureRect? badge = secondBadge.GetNodeOrNull<TextureRect>(_nodeName);
            if (badge == null)
            {
                badge = GD.Load<PackedScene>("res://scenes/ui/top_bar/second_boss_icon.tscn")
                    .Instantiate<TextureRect>(PackedScene.GenEditState.Disabled);

                badge.Name = _nodeName;
                badge.MouseFilter = Control.MouseFilterEnum.Pass;
                badge.GetNode<TextureRect>("%Outline").MouseFilter = Control.MouseFilterEnum.Pass;

                secondBadge.AddChild(badge);
                badge.Position = _offset;
            }

            badge.Texture = PreloadManager.Cache.GetTexture2D(icon);
            badge.GetNode<TextureRect>("%Outline").Texture = PreloadManager.Cache.GetTexture2D(outline);
            badge.Visible = true;
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Could not show the third boss on the top bar: {e}");
        }
    }

    private static TextureRect? SecondBadge(TextureRect main)
    {
        return main.GetChildren()
            .OfType<TextureRect>()
            .FirstOrDefault(child => child.Name != "Outline" && child.Name != _nodeName);
    }

    private static TextureRect? Badge(TextureRect main)
    {
        return SecondBadge(main)?.GetNodeOrNull<TextureRect>(_nodeName);
    }
}

// The hover tip has a one-boss and a two-boss wording and no more, so a third gets its own from the mod's strings.
[HarmonyPatch(typeof(NTopBarBossIcon), "OnFocus")]
internal static class TripleBossHoverTipPatch
{
    [HarmonyPrefix]
    private static bool DescribeThreeBosses(NTopBarBossIcon __instance)
    {
        try
        {
            if (TripleBoss.Run is not { } run
                || run.Act.SecondBossEncounter is not { } second
                || TripleBossMap.Third(run.Map) == null
                || run.CurrentMapPoint == run.Map.BossMapPoint
                || TripleBoss.ThirdBoss(run.Act) is not { } third)
            {
                return true;
            }

            LocString title = new("static_hover_tips", "TRIPLE_BOSS.title");
            LocString description = new("static_hover_tips", "TRIPLE_BOSS.description");

            foreach (LocString line in new[] { title, description })
            {
                line.Add("BossName1", run.Act.BossEncounter.Title);
                line.Add("BossName2", second.Title);
                line.Add("BossName3", third.Title);
            }

            TextureRect main = __instance.GetNode<TextureRect>("Icon");
            NHoverTipSet.CreateAndShow(__instance, new HoverTip(title, description))
                ?.SetGlobalPosition(main.GlobalPosition + new Vector2(0f, __instance.Size.Y + 20f));

            return false;
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Could not describe the third boss: {e}");
            return true;
        }
    }
}

[HarmonyPatch(typeof(NTopBarBossIcon), nameof(NTopBarBossIcon.RefreshBossIcon))]
internal static class TripleBossIconPatch
{
    [HarmonyPostfix]
    private static void OverrideWithThirdBoss(NTopBarBossIcon __instance)
    {
        TripleBossTopBar.ShowThirdBoss(__instance);
    }
}

[HarmonyPatch(typeof(NTopBarBossIcon), "OnRoomEntered")]
internal static class TripleBossIconVisibilityPatch
{
    [HarmonyPostfix]
    private static void KeepVisibleForThirdBoss(NTopBarBossIcon __instance)
    {
        TripleBossTopBar.ShowThirdBoss(__instance);
    }
}
