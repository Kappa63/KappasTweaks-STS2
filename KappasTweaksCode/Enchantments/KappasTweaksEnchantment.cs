using BaseLib.Abstracts;

namespace KappasTweaks.KappasTweaksCode.Enchantments;

// No CustomIconPath: BaseLib's IconPath patch can get bypassed by inlining, so icons live where vanilla already looks,
// res://images/enchantments/<id>.png (e.g. images/enchantments/kappastweaks-one_more_time.png).
public abstract class KappasTweaksEnchantment : CustomEnchantmentModel;
