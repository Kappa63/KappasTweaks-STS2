using KappasTweaks.KappasTweaksCode.Domino.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Powers;

public sealed class BlankCheckPower : BonesmanPower, IChainListener
{
    private bool _usedThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
            _usedThisTurn = false;
        return Task.CompletedTask;
    }

    public async Task OnChainBroken(PlayerChoiceContext context, BonesmanCard card)
    {
        if (_usedThisTurn)
            return;
        _usedThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player!);
    }
}
