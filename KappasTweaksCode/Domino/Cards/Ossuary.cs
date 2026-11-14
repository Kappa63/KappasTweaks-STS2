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
public sealed class Ossuary() : BonesmanCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    // HYPER
    private const int _amount = 2;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new PowerVar<OssuaryPower>(_amount),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await PowerCmd.Apply<OssuaryPower>(choiceContext, Owner.Creature, DynamicVars[nameof(OssuaryPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(OssuaryPower)].UpgradeValueBy(1);
    }
}
