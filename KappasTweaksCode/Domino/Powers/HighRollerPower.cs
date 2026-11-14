using KappasTweaks.KappasTweaksCode.Domino.Cards;
using KappasTweaks.KappasTweaksCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Powers;

public sealed class HighRollerPower : BonesmanPower, IChainListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnDominoPlayed(PlayerChoiceContext context, BonesmanCard card)
    {
        if (CombatState?.HittableEnemies.ToList() is not { Count: > 0 } enemies || card.PipLeft + card.PipRight <= 0)
            return;
        Flash();
        Creature target = enemies[Owner.Player!.RunState.Rng.CombatTargets.NextInt(enemies.Count)];
        await CreatureCmd.Damage(context, [target], (card.PipLeft + card.PipRight) * Amount, ValueProp.Unpowered, Owner);
    }
}
