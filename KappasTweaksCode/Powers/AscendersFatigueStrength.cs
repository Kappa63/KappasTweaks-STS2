using BaseLib.Abstracts;
using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KappasTweaks.KappasTweaksCode.Powers;

// Inherits the game's TemporaryStrengthPower so it wears off at end of turn like Anticipate does, and implements
// ICustomPower for BaseLib's id prefix + icon redirect -- CustomPowerModel can't be inherited alongside it.
// Title/Description come from the base class (OriginModel + TEMPORARY_STRENGTH_DOWN), so this needs no loc.
public sealed class AscendersFatigueStrength : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<AscendersFatigue>();

    protected override bool IsPositive => false;

    // Borrow the base game's Strength art; this is a Strength debuff, and it saves shipping an icon.
    // public string? CustomPackedIconPath => ModelDb.Power<StrengthPower>().PackedIconPath;
    //
    // public string? CustomBigIconPath => ModelDb.Power<StrengthPower>().ResolvedBigIconPath;
}
