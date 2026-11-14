using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class PipShot() : BonesmanCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    // HYPER
    private const int _flat = 3;
    private const int _flatUpgrade = 3;
    private const int _cards = 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [BonesmanKeywords.Jumbled];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new IntVar(BonusKey, _flat),
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, PipTotal + Bonus);
        if (chained)
            await Draw(choiceContext, DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusKey].UpgradeValueBy(_flatUpgrade);
    }
}
