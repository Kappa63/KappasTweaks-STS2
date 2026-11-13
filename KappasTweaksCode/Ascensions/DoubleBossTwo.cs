using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[Ascension(Level)]
internal static class DoubleBossTwo
{
    // HYPER
    internal const int Level = 16;

    internal const AscensionLevel Ascension = (AscensionLevel)Level;
}

[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
internal static class DoubleBossTwoRoomsPatch
{
    [HarmonyPostfix]
    private static void AddSecondBossToActTwo(ActModel __instance, Rng rng)
    {
        if (__instance.Index != 1 || __instance.HasSecondBoss)
            return;

        AscensionManager? ascension = RunManager.Instance.AscensionManager;
        if (ascension == null || !ascension.HasLevel(DoubleBossTwo.Ascension))
            return;

        EncounterModel? second =
            rng.NextItem(__instance.AllBossEncounters.Where(e => e.Id != __instance.BossEncounter.Id));

        if (second != null)
            __instance.SetSecondBossEncounter(second);
    }
}
