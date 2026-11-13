using BaseLib.Abstracts;
using BaseLib.Extensions;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Relics;

public abstract class KappasTweaksRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}