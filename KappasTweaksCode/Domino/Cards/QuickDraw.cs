using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class QuickDraw() : BonesmanCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    // HYPER
    private const int _cards = 2;
    private const int _cardsUpgrade = 1;
    private const int _bonus = 1;

    protected override (int Left, int Right)? FixedPips => (1, 3);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_cards),
        new IntVar(BonusKey, _bonus),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Draw(choiceContext, DynamicVars.Cards.BaseValue + (chained ? Bonus : 0));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_cardsUpgrade);
    }
}
