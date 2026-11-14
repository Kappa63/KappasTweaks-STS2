using BaseLib.Abstracts;
using BaseLib.Extensions;
using KappasTweaks.KappasTweaksCode.Cards;
using KappasTweaks.KappasTweaksCode.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KappasTweaks.KappasTweaksCode.Powers;

// See AscendersFatigueStrength for why this inherits the game power plus ICustomPower.
public sealed class AscendersFatigueDexterity : TemporaryDexterityPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<AscendersFatigue>();

    protected override bool IsPositive => false;

    public string? CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public string? CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}
