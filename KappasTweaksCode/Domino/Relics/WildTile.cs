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

// Lends a one-turn Perfect Match, so the first domino each turn that would break Chains instead.
[Pool(typeof(BonesmanRelicPool))]
public sealed class WildTile : KappasTweaksRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
            return;
        await PowerCmd.Apply<PerfectMatchPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null, silent: true);
    }
}
