using System.Reflection;
using Godot;
using HarmonyLib;
using KappasTweaks.KappasTweaksCode.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace KappasTweaks.KappasTweaksCode;

internal static class BaseGameArtPatch
{

    private static void Redirect(string folder, string suffix, ref string result)
    {
        string name = Path.GetFileNameWithoutExtension(result);
        if (string.IsNullOrEmpty(name))
            return;

        string candidate = $"{folder}/{name}{suffix}.png".ImagePath();
        if (ResourceLoader.Exists(candidate))
            result = candidate;
    }

    private static string Redirected(string folder, string suffix, string path)
    {
        string result = path;
        Redirect(folder, suffix, ref result);
        return result;
    }

    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.PackedIconPath), MethodType.Getter)]
    internal static class RelicIcon
    {
        [HarmonyPostfix]
        private static void Replace(ref string __result)
        {
            Redirect("relics", "", ref __result);
        }
    }

    [HarmonyPatch(typeof(RelicModel), "PackedIconOutlinePath", MethodType.Getter)]
    internal static class RelicOutline
    {
        [HarmonyPostfix]
        private static void Replace(ref string __result)
        {
            Redirect("relics", "_outline", ref __result);
        }
    }

    [HarmonyPatch(typeof(RelicModel), "BigIconPath", MethodType.Getter)]
    internal static class RelicBigIcon
    {
        [HarmonyPostfix]
        private static void Replace(ref string __result)
        {
            Redirect("relics/big", "", ref __result);
        }
    }

    [HarmonyPatch]
    internal static class CardPortrait
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.DeclaredPropertyGetter(typeof(CardModel), nameof(CardModel.PortraitPath));
            yield return AccessTools.DeclaredPropertyGetter(typeof(Wither), nameof(CardModel.PortraitPath));
            yield return AccessTools.DeclaredPropertyGetter(typeof(MadScience), nameof(CardModel.PortraitPath));
        }

        [HarmonyPostfix]
        private static void Replace(ref string __result)
        {
            Redirect("card_portraits", "", ref __result);
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.AllPortraitPaths), MethodType.Getter)]
    internal static class CardAllPortraits
    {
        [HarmonyPostfix]
        private static void Replace(ref IEnumerable<string> __result)
        {
            __result = __result.Select(p => Redirected("card_portraits", "", p)).ToArray();
        }
    }

    [HarmonyPatch]
    internal static class CardAllPortraitsArray
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.DeclaredPropertyGetter(typeof(Wither), nameof(CardModel.AllPortraitPaths));
            yield return AccessTools.DeclaredPropertyGetter(typeof(MadScience), nameof(CardModel.AllPortraitPaths));
        }

        [HarmonyPostfix]
        private static void Replace(ref string[] __result)
        {
            __result = __result.Select(p => Redirected("card_portraits", "", p)).ToArray();
        }
    }
}
