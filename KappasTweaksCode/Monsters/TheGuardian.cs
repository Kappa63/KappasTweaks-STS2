using KappasTweaks.KappasTweaksCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Monsters;

public class TheGuardian : KappasTweaksMonster
{
    private bool _isDefensiveMode;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 250, 240);
    public override int MaxInitialHp => MinInitialHp;

    protected override string DefaultSpriteState => "attack_mode";
    protected override IEnumerable<string> SpriteStates => ["defensive_mode"];
    protected override float VisualScale => 0.7f;

    private int ModeShiftThreshold => 30;
    private int ChargingUpAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 9);
    private int FierceBashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 36, 32);
    private int VentSteamAmount => 2;
    private int WhirlwindDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int WhirlwindRepeat => 4;
    private int DefensiveModeAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int RollAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int TwinSlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int TwinSlamRepeat => 2;

    public bool IsDefensiveMode
    {
        get => _isDefensiveMode;
        set
        {
            AssertMutable();
            _isDefensiveMode = value;
            SpriteState = value ? "defensive_mode" : null;
        }
    }

    private MoveState? _defensiveModeState;

    public MoveState? DefensiveModeState
    {
        get => _defensiveModeState;
        private set
        {
            AssertMutable();
            _defensiveModeState = value;
        }
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<ModeShift>(new ThrowingPlayerChoiceContext(), Creature, ModeShiftThreshold, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState chargingUp = new("CHARGING_UP", ChargingUpMove, new DefendIntent());
        MoveState fierceBash = new("FIERCE_BASH", FierceBashMove, new SingleAttackIntent(FierceBashDamage));
        MoveState ventSteam = new("VENT_STEAM", VentSteamMove, new DebuffIntent(strong: true));
        MoveState whirlwind = new("WHIRLWIND", WhirlwindMove, new MultiAttackIntent(WhirlwindDamage, WhirlwindRepeat));
        MoveState defensiveMode = new("DEFENSIVE_MODE", DefensiveModeMove, new BuffIntent());
        MoveState rollAttack = new("ROLL_ATTACK", RollAttackMove, new SingleAttackIntent(RollAttackDamage));
        MoveState twinSlam = new("TWIN_SLAM", TwinSlamMove, new MultiAttackIntent(TwinSlamDamage, TwinSlamRepeat));

        chargingUp.FollowUpState = fierceBash;
        fierceBash.FollowUpState = ventSteam;
        ventSteam.FollowUpState = whirlwind;
        whirlwind.FollowUpState = chargingUp;

        defensiveMode.FollowUpState = rollAttack;
        rollAttack.FollowUpState = twinSlam;
        twinSlam.FollowUpState = whirlwind;

        DefensiveModeState = defensiveMode;

        List<MonsterState> states = [chargingUp, fierceBash, ventSteam, whirlwind, defensiveMode, rollAttack, twinSlam];
        return new MonsterMoveStateMachine(states, chargingUp);
    }

    private async Task ChargingUpMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, ChargingUpAmount, ValueProp.Move, null);
    }

    private async Task FierceBashMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(FierceBashDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task VentSteamMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, -VentSteamAmount, Creature, null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, -VentSteamAmount, Creature, null);
    }

    private async Task WhirlwindMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(WhirlwindDamage).WithHitCount(WhirlwindRepeat).FromMonster(this)
            .WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task DefensiveModeMove(IReadOnlyList<Creature> targets)
    {
        IsDefensiveMode = true;
        await PowerCmd.Apply<SharpHide>(new ThrowingPlayerChoiceContext(), Creature, DefensiveModeAmount, Creature, null);
    }

    private async Task RollAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(RollAttackDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task TwinSlamMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TwinSlamDamage).WithHitCount(TwinSlamRepeat).FromMonster(this)
            .WithHitFx("vfx/vfx_attack_slash").Execute(null);
        await PowerCmd.Remove<SharpHide>(Creature);
        IsDefensiveMode = false;
    }
}
