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

// Took over Count the Pips' art (a finger on a domino).
[Pool(typeof(BonesmanCardPool))]
public sealed class DoubleUp() : BonesmanCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    // HYPER
    private const int _cards = 1;
    private const int _cardsUpgrade = 1;

    protected override bool IsDomino => false;
    protected override string ArtName => "count_the_pips";

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        BonesmanCard? best = Owner.PlayerCombatState!.Hand.Cards.OfType<BonesmanCard>()
            .Where(c => c.Domino && c.PipLeft >= 0 && !c.IsDouble)
            .OrderByDescending(c => c.PipValue)
            .FirstOrDefault();
        best?.MakeDouble();
        await Draw(choiceContext, DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_cardsUpgrade);
    }
}
