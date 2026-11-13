using MegaCrit.Sts2.Core.Entities.Ascension;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[Ascension(Level)]
[LowerMaxHpAt(0)]
internal static class LowerMaxHpOne
{
    // HYPER
    internal const int Level = 12;

    internal const AscensionLevel Ascension = (AscensionLevel)Level;
}
