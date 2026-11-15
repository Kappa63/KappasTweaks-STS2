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

// Breaker: cash the streak in for damage to everything.
[Pool(typeof(BonesmanCardPool))]
public sealed class CleanSweep() : BonesmanCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    // HYPER
    private const int _perLink = 4;
    private const int _perLinkUpgrade = 2;

    protected override bool IsDomino => false;

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(0),
        new ExtraDamageVar(_perLink),
        ScaledDamage(c => c.ChainCount),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        if (ChainCount > 0)
            await HitAllScaled(choiceContext, play);
        await BreakChain(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(_perLinkUpgrade);
    }
}
