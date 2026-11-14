using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace KappasTweaks.KappasTweaksCode.Domino;

public static class BonesmanKeywords
{
    // Never sits on a card; only lends its hover tip to the "Chained:" line.
    [CustomEnum] public static CardKeyword Chained;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Jumbled;
}
