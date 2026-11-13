using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

// HYPER
using FatigueCard = KappasTweaks.KappasTweaksCode.Cards.AscendersFatigue;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[Ascension(Level)]
internal static class AscendersFatigue
{
    // HYPER
    internal const int Level = 11;

    internal const AscensionLevel Ascension = (AscensionLevel)Level;
}

// ApplyEffectsTo is where ascension 5 puts Ascender's Bane in the deck, and it runs once at run start (not on
// load), so this postfix swaps that curse for the harsher one instead of stacking a second copy.
[HarmonyPatch(typeof(AscensionManager), nameof(AscensionManager.ApplyEffectsTo))]
internal static class AscendersFatigueDeckPatch
{
    [HarmonyPostfix]
    private static void ReplaceAscendersBane(AscensionManager __instance, Player player)
    {
        if (!__instance.HasLevel(AscendersFatigue.Ascension))
            return;

        foreach (CardModel bane in player.Deck.Cards.Where(c => c is AscendersBane).ToList())
            player.Deck.RemoveInternal(bane, silent: true);

        FatigueCard fatigue = player.RunState.CreateCard<FatigueCard>(player);
        fatigue.FloorAddedToDeck = 1;
        player.Deck.AddInternal(fatigue, -1, silent: true);
    }
}
