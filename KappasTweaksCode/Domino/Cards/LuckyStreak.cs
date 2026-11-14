using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using KappasTweaks.KappasTweaksCode.Domino.Powers;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class LuckyStreak() : BonesmanCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    // HYPER
    private const int _amount = 3;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new PowerVar<LuckyStreakPower>(_amount),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await PowerCmd.Apply<LuckyStreakPower>(choiceContext, Owner.Creature, DynamicVars[nameof(LuckyStreakPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LuckyStreakPower)].UpgradeValueBy(1);
    }
}
