using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

// A count shown on the card in combat (hits, cards, chips), recomputed whenever the card redraws. Shown in white:
// it's information, not a buff.
public sealed class LiveVar(string name, Func<BonesmanCard, decimal> value) : DynamicVar(name, 0m)
{
    public decimal Of(BonesmanCard card) => value(card);

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        if (card is BonesmanCard domino)
            EnchantedValue = PreviewValue = value(domino);
    }
}
