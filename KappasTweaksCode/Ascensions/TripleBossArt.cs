using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

// NBossMapPoint._Ready picks its art with a binary test -- "am I the second boss point, else I am the first" -- so a
// third node falls into the else and draws the first boss. Redo that choice once the node is built.
[HarmonyPatch(typeof(NBossMapPoint), "_Ready")]
internal static class TripleBossNodeArtPatch
{
    [HarmonyPostfix]
    private static void UseThirdBossArt(NBossMapPoint __instance)
    {
        try
        {
            if (TripleBoss.Run is not { } run
                || TripleBossMap.Third(run.Map) is not { } third
                || !ReferenceEquals(__instance.Point, third)
                || TripleBoss.ThirdBoss(run.Act) is not { } encounter)
            {
                return;
            }

            // Handing the Spine binding a resource built from an uncached asset kills the process natively, which no
            // catch here can stop. If the art was not preloaded, leave whatever vanilla drew and take a wrong icon
            // over a crash.
            string path = encounter.BossNodePath;
            bool usesSpine = ResourceLoader.Exists(path);
            if (!PreloadManager.Cache.ContainsKey(usesSpine ? path : path + ".png"))
            {
                MainFile.Logger.Info($"Third boss art not preloaded, leaving the vanilla icon: {path}");
                return;
            }

            Node2D spine = __instance.GetNode<Node2D>("%SpineSprite");
            TextureRect image = __instance.GetNode<TextureRect>("%PlaceholderImage");
            TextureRect outline = __instance.GetNode<TextureRect>("%PlaceholderOutline");

            if (usesSpine && encounter.BossNodeSpineResource is { } skeleton)
            {
                MegaSprite anim = _animController(__instance);
                anim.SetSkeletonDataRes(skeleton);

                spine.Visible = true;
                image.Visible = false;
                _usesSpine(__instance) = true;

                // set_skeleton_data_res builds the skeleton asynchronously, so neither the animation state nor the
                // shader material exists yet -- driving them straight from this _Ready postfix is a native crash, not
                // a catchable one. _material also never got assigned, because vanilla only sets it on the Spine branch
                // and reaching here means it took the other one, and RefreshColorInstantly dereferences it as soon as
                // _usesSpine is true. So both wait for the skeleton, and so does the refresh.
                __instance.RunWhenSpineReady(anim, state =>
                {
                    if (anim.HasAnimation("animation"))
                        state.AddAnimation("animation");

                    if (anim.GetNormalMaterial() is ShaderMaterial material)
                        _material(__instance) = material;

                    _refreshColor(__instance);
                });

                return;
            }

            image.Texture = PreloadManager.Cache.GetAsset<Texture2D>(encounter.BossNodePath + ".png");

            // Only the base icon was checked above; an uncached outline would be a synchronous load of a file that may
            // not exist, which dies in Godot's resource parser rather than returning null.
            string outlinePath = encounter.BossNodePath + "_outline.png";
            if (PreloadManager.Cache.ContainsKey(outlinePath))
            {
                outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>(outlinePath);
            }

            // Vanilla only assigns these two on the branch it did not take to get here, so RefreshColorInstantly would
            // dereference nulls the moment _usesSpine goes false. Mirror of the _material case on the Spine side.
            _placeholderImage(__instance) = image;
            _placeholderOutline(__instance) = outline;

            image.SelfModulate = run.Act.MapTraveledColor;
            outline.SelfModulate = run.Act.MapBgColor;

            spine.Visible = false;
            image.Visible = true;
            _usesSpine(__instance) = false;

            _refreshColor(__instance);
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Could not apply the third boss node art: {e}");
        }
    }

    private static readonly AccessTools.FieldRef<NBossMapPoint, bool> _usesSpine =
        AccessTools.FieldRefAccess<NBossMapPoint, bool>("_usesSpine");

    private static readonly AccessTools.FieldRef<NBossMapPoint, MegaSprite> _animController =
        AccessTools.FieldRefAccess<NBossMapPoint, MegaSprite>("_animController");

    private static readonly AccessTools.FieldRef<NBossMapPoint, ShaderMaterial> _material =
        AccessTools.FieldRefAccess<NBossMapPoint, ShaderMaterial>("_material");

    private static readonly AccessTools.FieldRef<NBossMapPoint, TextureRect> _placeholderImage =
        AccessTools.FieldRefAccess<NBossMapPoint, TextureRect>("_placeholderImage");

    private static readonly AccessTools.FieldRef<NBossMapPoint, TextureRect> _placeholderOutline =
        AccessTools.FieldRefAccess<NBossMapPoint, TextureRect>("_placeholderOutline");

    private static readonly Action<NBossMapPoint> _refreshColor =
        AccessTools.MethodDelegate<Action<NBossMapPoint>>(
            AccessTools.Method(typeof(NBossMapPoint), "RefreshColorInstantly"));
}
