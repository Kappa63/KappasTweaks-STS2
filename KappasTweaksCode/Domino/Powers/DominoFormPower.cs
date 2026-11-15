using KappasTweaks.KappasTweaksCode.Domino.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Powers;

// At the end of your turn, plays every domino left in hand that matches an open end, in order, for free.
public sealed class DominoFormPower : BonesmanPower
{
    // HYPER
    private const int _maxPlays = 20;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;
        for (int i = 0; i < _maxPlays; i++)
        {
            BonesmanCard? next = PileType.Hand.GetPile(player).Cards.OfType<BonesmanCard>()
                .FirstOrDefault(c => c.Matches && !c.Keywords.Contains(CardKeyword.Unplayable));
            if (next == null)
                return;
            if (i == 0)
                Flash();
            await CardCmd.AutoPlay(choiceContext, next, null);
        }
    }
}
