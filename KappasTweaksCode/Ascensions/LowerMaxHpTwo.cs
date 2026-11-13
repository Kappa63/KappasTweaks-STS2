using MegaCrit.Sts2.Core.Entities.Ascension;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[Ascension(Level)]
[LowerMaxHpAt(1)]
internal static class LowerMaxHpTwo
{
    // HYPER
    internal const int Level = 13;

    internal const AscensionLevel Ascension = (AscensionLevel)Level;
}
