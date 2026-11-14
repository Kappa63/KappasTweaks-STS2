using BaseLib.Abstracts;
using BaseLib.Extensions;
using KappasTweaks.KappasTweaksCode.Cards;
using KappasTweaks.KappasTweaksCode.Extensions;
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

    public string? CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public string? CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}
