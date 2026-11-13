using BaseLib.Abstracts;
using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KappasTweaks.KappasTweaksCode.Powers;

// See AscendersFatigueStrength for why this inherits the game power plus ICustomPower.
public sealed class AscendersFatigueDexterity : TemporaryDexterityPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<AscendersFatigue>();

    protected override bool IsPositive => false;

    // public string? CustomPackedIconPath => ModelDb.Power<DexterityPower>().PackedIconPath;
    //
    // public string? CustomBigIconPath => ModelDb.Power<DexterityPower>().ResolvedBigIconPath;
}
