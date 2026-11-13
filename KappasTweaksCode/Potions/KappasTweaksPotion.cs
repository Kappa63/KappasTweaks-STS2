using BaseLib.Abstracts;
using BaseLib.Extensions;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Potions;

public abstract class KappasTweaksPotion : CustomPotionModel
{
    public override string CustomPackedImagePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();

    public override string CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".PotionOutlineImagePath();
}
