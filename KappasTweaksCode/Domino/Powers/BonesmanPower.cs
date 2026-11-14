using BaseLib.Extensions;
using KappasTweaks.KappasTweaksCode.Powers;

namespace KappasTweaks.KappasTweaksCode.Domino.Powers;

public abstract class BonesmanPower : KappasTweaksPower
{
    private string ArtName => Id.Entry.RemovePrefix().ToLowerInvariant();
    public override string CustomPackedIconPath => BonesmanArt.PowerIcon(ArtName, GetType().Name, big: false);
    public override string CustomBigIconPath => BonesmanArt.PowerIcon(ArtName, GetType().Name, big: true);
}
