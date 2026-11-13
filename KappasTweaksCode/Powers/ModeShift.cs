using KappasTweaks.KappasTweaksCode.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Powers;

public class ModeShift : KappasTweaksPower
{
    private const int _blockOnShift = 20;
    private const int _thresholdEscalation = 10;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => true;

    public override int DisplayAmount => (int)Math.Max(0m, Amount - GetInternalData<Data>().damageTaken);

    protected override object InitInternalData() => new Data();

    private class Data
    {
        public decimal damageTaken;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner
            || result.WasFullyBlocked
            || Owner.Monster is not TheGuardian { IsDefensiveMode: false } guardian
            || guardian.DefensiveModeState is not { } defensiveMode)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.damageTaken += result.UnblockedDamage;
        InvokeDisplayAmountChanged();

        if (data.damageTaken < Amount)
            return;

        Flash();
        data.damageTaken = 0m;
        SetAmount(Amount + _thresholdEscalation);

        await CreatureCmd.GainBlock(Owner, _blockOnShift, ValueProp.Unpowered, null);
        guardian.IsDefensiveMode = true;
        guardian.SetMoveImmediate(defensiveMode, forceTransition: true);
    }
}
