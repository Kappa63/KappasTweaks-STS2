using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace KappasTweaks.KappasTweaksCode.Cards;


[Pool(typeof(DefectCardPool))]
public class Recursion() : KappasTweaksCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    private const int _energyUpgrade = 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Evoke),
        HoverTipFactory.Static(StaticHoverTip.Channeling)
    ];

    protected override async Task OnPlay( PlayerChoiceContext choiceContext, CardPlay play)
    {
        Recursion card = this;
        int orbCount = card.Owner.PlayerCombatState!.OrbQueue.Orbs.Count;
        if (orbCount == 0)
            return;
        OrbModel orbToChannel = card.Owner.PlayerCombatState.OrbQueue.Orbs[0];
        await OrbCmd.EvokeNext(choiceContext, card.Owner);
        await OrbCmd.Channel(choiceContext, orbToChannel, card.Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-_energyUpgrade);
    }
}