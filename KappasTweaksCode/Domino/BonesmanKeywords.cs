using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace KappasTweaks.KappasTweaksCode.Domino;

public static class BonesmanKeywords
{
    // Never sits on a card; only lends its hover tip to the "Chained:" line.
    [CustomEnum] public static CardKeyword Chained;
    [CustomEnum] public static CardKeyword Wild;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Jumbled;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Loose;
}
