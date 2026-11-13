using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Ascensions;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class LowerMaxHpAtAttribute(int actIndex) : Attribute
{
    internal int ActIndex { get; } = actIndex;
}

internal static class LowerMaxHp
{
    internal const decimal Multiplier = 0.9m;

    private static readonly (int Act, AscensionLevel Level)[] _stages = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Select(t => (At: t.GetCustomAttribute<LowerMaxHpAtAttribute>(), Asc: t.GetCustomAttribute<AscensionAttribute>()))
        .Where(t => t is { At: not null, Asc: not null })
        .Select(t => (t.At!.ActIndex, (AscensionLevel)t.Asc!.Level))
        .ToArray();

    internal static bool AppliesTo(int actIndex)
    {
        return _stages.Any(s => s.Act == actIndex && AscensionHelper.HasAscension(s.Level));
    }
}

[HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted")]
internal static class LowerMaxHpPatch
{
    [HarmonyPrefix]
    private static void TrimMaxHp(AncientEventModel __instance, bool isPreFinished)
    {
        if (isPreFinished || __instance.Owner is not { } owner)
            return;

        IRunState run = owner.RunState;
        if (run.CurrentMapCoord != run.Map.StartingMapPoint.coord || !LowerMaxHp.AppliesTo(run.CurrentActIndex))
            return;

        owner.Creature.SetMaxHpInternal(owner.Creature.MaxHp * LowerMaxHp.Multiplier);
    }
}
