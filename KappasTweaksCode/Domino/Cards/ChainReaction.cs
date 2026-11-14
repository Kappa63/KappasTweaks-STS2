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
public sealed class ChainReaction() : BonesmanCard(1, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    // HYPER
    private const int _amount = 2;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new PowerVar<ChainReactionPower>(_amount),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await PowerCmd.Apply<ChainReactionPower>(choiceContext, Owner.Creature, DynamicVars[nameof(ChainReactionPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(ChainReactionPower)].UpgradeValueBy(1);
    }
}
