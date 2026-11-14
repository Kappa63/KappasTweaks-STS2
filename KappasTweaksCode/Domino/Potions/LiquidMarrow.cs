using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using KappasTweaks.KappasTweaksCode.Potions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Potions;

[Pool(typeof(BonesmanPotionPool))]
public sealed class LiquidMarrow : KappasTweaksPotion
{
    // HYPER
    private const int _perLink = 5;
    private const int _minimum = 8;

    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerLink", _perLink), new BlockVar(_minimum, ValueProp.Unpowered)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        int block = Math.Max(_minimum, _perLink * (Chain.Of(Owner)?.Links ?? 0));
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, null);
    }
}
