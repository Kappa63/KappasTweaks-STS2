using BaseLib.Abstracts;
using BaseLib.Extensions;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Enchantments;

public abstract class KappasTweaksEnchantment : CustomEnchantmentModel
{
    protected override string CustomIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".EnchantmentImagePath();
}