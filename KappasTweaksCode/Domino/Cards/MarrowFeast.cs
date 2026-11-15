using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class MarrowFeast() : BonesmanCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    // HYPER
    private const int _cards = 1;

    protected override bool IsDomino => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> CardHoverTips => [HoverTipFactory.FromCard<BoneChip>()];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        List<CardModel> chips = Owner.PlayerCombatState!.Hand.Cards.Where(c => c is BoneChip).ToList();
        foreach (CardModel chip in chips)
            await CardCmd.Exhaust(choiceContext, chip);
        if (chips.Count == 0)
            return;
        await PlayerCmd.GainEnergy(chips.Count, Owner);
        await Draw(choiceContext, chips.Count * DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
