using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class LinedUp() : BonesmanCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    // HYPER
    private const int _cards = 1;
    private const int _cardsUpgrade = 1;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        List<BonesmanCard> dominoes = Owner.PlayerCombatState!.Hand.Cards.OfType<BonesmanCard>().Where(c => c != this && c.Domino && c.PipLeft >= 0).ToList();
        if (dominoes.Count > 0)
        {
            BonesmanCard pick = dominoes[Owner.RunState.Rng.CombatCardSelection.NextInt(dominoes.Count)];
            (await Chain.Ensure(choiceContext, Owner)).SetEnds(pick.PipLeft, pick.PipRight);
        }
        await Draw(choiceContext, DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_cardsUpgrade);
    }
}
