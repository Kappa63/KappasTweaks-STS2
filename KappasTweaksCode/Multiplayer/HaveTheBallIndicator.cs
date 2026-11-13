using MegaCrit.Sts2.Core.Entities.Players;

// HYPER
using BallCard = MegaCrit.Sts2.Core.Models.Cards.TheBall;

namespace KappasTweaks.KappasTweaksCode.Multiplayer;

// TheBall hands itself to a random teammate's draw pile after every play, so it is worth knowing who is holding it.
public sealed class HaveTheBallIndicator : KappasTweaksLocationIcon
{
    protected override bool IsActive(Player player)
    {
        if (player.PlayerCombatState is not { } combat)
            return false;

        return new[] { combat.Hand, combat.DrawPile, combat.DiscardPile }
            .Any(pile => pile.Cards.Any(card => card is BallCard));
    }
}
