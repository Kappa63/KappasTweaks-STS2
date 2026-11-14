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
public sealed class BottledBlank : KappasTweaksPotion
{
    // HYPER
    private const int _cards = 2;

    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(_cards)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        (await Chain.Ensure(choiceContext, Owner)).SetEnds(ChainPower.Blank, ChainPower.Blank);
        await CardPileCmd.Draw(choiceContext, _cards, Owner);
    }
}
