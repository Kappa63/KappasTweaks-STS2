using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

// TUNABLE: the potion this relic hands out. It has to live up here rather than with the other
// tunables in the class -- it's used as a generic type argument, and C# only aliases types via a
// file-scope using. Swapping this one line redirects every use below.
using TargetPotion = KappasTweaks.KappasTweaksCode.Potions.BrokenNuke;

namespace KappasTweaks.KappasTweaksCode.Relics;

[Pool(typeof(SharedRelicPool))]
public sealed class HahaBomba : KappasTweaksRelic
{
    // HYPER
    private const int _potionSlotsToAdd = 2;
    private const int _potionsToAdd = 4;
    private const string _potionSlotsKey = "PotionSlots";
    private const string _potionCountKey = "Potions";
    private const string _potionKey = "PotionName";

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(_potionSlotsKey, _potionSlotsToAdd),
        new IntVar(_potionCountKey, _potionsToAdd),
        new StringVar(_potionKey, ModelDb.Potion<TargetPotion>().Title.GetFormattedText()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<TargetPotion>()];

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainMaxPotionCount(DynamicVars[_potionSlotsKey].IntValue, Owner);

        for (int i = 0; i < DynamicVars[_potionCountKey].IntValue; i++)
            await PotionCmd.TryToProcure<TargetPotion>(Owner);
    }
}
