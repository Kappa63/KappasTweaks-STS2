using KappasTweaks.KappasTweaksCode.Domino.Cards;
using KappasTweaks.KappasTweaksCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Powers;

public sealed class DominoTheoryPower : BonesmanPower, IChainListener
{
    // HYPER
    private const int _linksNeeded = 3;
    private const int _energy = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnChained(PlayerChoiceContext context, BonesmanCard card)
    {
        if (Chain.Of(Owner.Player) is not { LinksThisTurn: _linksNeeded })
            return;
        Flash();
        await PlayerCmd.GainEnergy(_energy, Owner.Player!);
        await CardPileCmd.Draw(context, Amount, Owner.Player!);
    }
}
