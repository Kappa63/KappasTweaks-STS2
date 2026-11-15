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
public sealed class Joker() : BonesmanCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    // HYPER
    private const int _energy = 2;
    private const int _energyUpgrade = 1;

    protected override bool IsDomino => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> CardHoverTips => [HoverTipFactory.FromKeyword(BonesmanKeywords.Wild)];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new EnergyVar(_energy),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        (await Chain.Ensure(choiceContext, Owner)).SetEnds(ChainPower.Wild, ChainPower.Wild);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(_energyUpgrade);
    }
}
