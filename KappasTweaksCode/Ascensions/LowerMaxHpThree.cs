using MegaCrit.Sts2.Core.Entities.Ascension;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[Ascension(Level)]
[LowerMaxHpAt(2)]
internal static class LowerMaxHpThree
{
    // HYPER
    internal const int Level = 14;

    internal const AscensionLevel Ascension = (AscensionLevel)Level;
}
