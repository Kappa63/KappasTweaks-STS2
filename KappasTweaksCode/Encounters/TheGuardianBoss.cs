using KappasTweaks.KappasTweaksCode.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace KappasTweaks.KappasTweaksCode.Encounters;

public sealed class TheGuardianBoss() : KappasTweaksEncounter(RoomType.Boss)
{
    public override string CustomBgm => "event:/music/act1_boss_ceremonial_beast";

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<TheGuardian>()];

    public override bool IsValidForAct(ActModel act) => act.Index == 0;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [(ModelDb.Monster<TheGuardian>().ToMutable(), null)];
}
