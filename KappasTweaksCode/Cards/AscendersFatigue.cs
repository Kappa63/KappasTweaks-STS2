using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KappasTweaks.KappasTweaksCode.Cards;

// Ascender's Bane, but it saps you every time you draw it. Curses can't be enchanted (EnchantmentModel.CanEnchant
// rejects Status/Curse/Quest outright), so this replaces Bane at its ascension rather than enchanting it.
[Pool(typeof(CurseCardPool))]
public sealed class AscendersFatigue() : KappasTweaksCard(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    // HYPER
    private const int _statLoss = 1;

    public override bool CanBeGeneratedByModifiers => false;

    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldGlowRedInternal => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Eternal, CardKeyword.Unplayable, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(_statLoss),
        new PowerVar<DexterityPower>(_statLoss),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
            return;

        await PowerCmd.Apply<AscendersFatigueStrength>(choiceContext, Owner.Creature,
            DynamicVars.Strength.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<AscendersFatigueDexterity>(choiceContext, Owner.Creature,
            DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
    }
}
