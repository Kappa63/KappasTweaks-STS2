using BaseLib.Abstracts;
using KappasTweaks.KappasTweaksCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;


namespace KappasTweaks.KappasTweaksCode.RestSiteOptions;

public abstract class KappaTweaksRestSiteOption(Player owner) : CustomRestSiteOption(owner)
{
    public override string CustomIconPath => $"option_{OptionId.ToLowerInvariant()}.png".RestSiteOptionImagePath();
}