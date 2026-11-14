using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Domino.Cards;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using KappasTweaks.KappasTweaksCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Relics;

[Pool(typeof(BonesmanRelicPool))]
public sealed class WornPip : KappasTweaksRelic, IChainListener
{
    // HYPER
    private const int _cards = 2;
    private bool _usedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override Task BeforeCombatStart()
    {
        _usedThisCombat = false;
        return Task.CompletedTask;
    }

    public async Task OnChained(PlayerChoiceContext context, BonesmanCard card)
    {
        if (_usedThisCombat)
            return;
        _usedThisCombat = true;
        Flash();
        await CardPileCmd.Draw(context, _cards, Owner);
    }
}
