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

public sealed class BadBeatPower : KappasTweaksPower, IChainListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnChainBroken(PlayerChoiceContext context, BonesmanCard card)
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
    }
}
