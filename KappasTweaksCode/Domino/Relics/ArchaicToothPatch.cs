using HarmonyLib;
using KappasTweaks.KappasTweaksCode.Domino.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KappasTweaks.KappasTweaksCode.Domino.Relics;

// Archaic Tooth transcends each character's signature basic; Double Six becomes Boxcars.
[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
internal static class ArchaicToothPatch
{
    [HarmonyPostfix]
    private static void AddDoubleSix(Dictionary<ModelId, CardModel> __result)
    {
        __result.TryAdd(ModelDb.Card<DoubleSix>().Id, ModelDb.Card<Boxcars>());
    }
}
