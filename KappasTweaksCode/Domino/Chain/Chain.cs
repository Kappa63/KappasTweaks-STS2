using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KappasTweaks.KappasTweaksCode.Domino;

internal static class Chain
{
    public static ChainPower? Of(Player? player) => player?.Creature.GetPower<ChainPower>();

    // Anyone can end up holding dominoes, so the chain appears on first use instead of belonging to the character.
    public static async Task<ChainPower> Ensure(PlayerChoiceContext context, Player player)
    {
        return Of(player)
               ?? await PowerCmd.Apply<ChainPower>(context, player.Creature, 1, player.Creature, null, silent: true)
               ?? throw new InvalidOperationException("Chain power could not be applied");
    }
}
