using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[Ascension(Level)]
internal static class DoubleBossThree
{
    // HYPER
    internal const int Level = 17;

    internal const AscensionLevel Ascension = (AscensionLevel)Level;
}

[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
internal static class DoubleBossThreeRoomsPatch
{
    [HarmonyPostfix]
    private static void AddSecondBossToActOne(ActModel __instance, Rng rng)
    {
        if (__instance.Index != 0 || __instance.HasSecondBoss)
            return;

        AscensionManager? ascension = RunManager.Instance.AscensionManager;
        if (ascension == null || !ascension.HasLevel(DoubleBossThree.Ascension))
            return;

        EncounterModel? second =
            rng.NextItem(__instance.AllBossEncounters.Where(e => e.Id != __instance.BossEncounter.Id));

        if (second != null)
            __instance.SetSecondBossEncounter(second);
    }
}
