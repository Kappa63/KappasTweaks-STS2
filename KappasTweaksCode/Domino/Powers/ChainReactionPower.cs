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

public sealed class ChainReactionPower : BonesmanPower, IChainListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnChained(PlayerChoiceContext context, BonesmanCard card)
    {
        if (CombatState?.HittableEnemies.ToList() is not { Count: > 0 } enemies || Chain.Of(Owner.Player) is not { } chain)
            return;
        Flash();
        Creature target = enemies[Owner.Player!.RunState.Rng.CombatTargets.NextInt(enemies.Count)];
        await CreatureCmd.Damage(context, [target], chain.Links * Amount, ValueProp.Unpowered, Owner);
    }
}
