using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class AllIn() : BonesmanCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    // HYPER
    private const int _per = 3;
    private const int _perUpgrade = -1;

    protected override bool IsDomino => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new IntVar(BonusKey, _per),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        int energy = (Chain.Of(Owner)?.Links ?? 0) / (int)Bonus;
        if (energy > 0)
            await PlayerCmd.GainEnergy(energy, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusKey].UpgradeValueBy(_perUpgrade);
    }
}
