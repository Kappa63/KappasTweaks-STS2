using System.Collections.Concurrent;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace KappasTweaks.KappasTweaksCode;

// BaseLib prefixes mod IDs by patching ModelDb.GetEntry, but GetEntry is small enough to be inlined into GetId. When
// GetId gets compiled before that patch lands, it hands out unprefixed IDs (RELIC.FEAR_THE_REAPER) and the v0.113 pool
// lookup throws on every relic hover. Invoking GetEntry through reflection always reaches the patched version.
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.GetId), [typeof(Type)])]
internal static class ModelIdPrefixFix
{
    private static readonly MethodInfo _getEntry = AccessTools.Method(typeof(ModelDb), nameof(ModelDb.GetEntry));
    private static readonly ConcurrentDictionary<Type, string> _entries = new();

    [HarmonyPostfix]
    private static void KeepPrefix(Type type, ref ModelId __result)
    {
        string entry = _entries.GetOrAdd(type, t => (string)_getEntry.Invoke(null, [t])!);
        if (entry != __result.Entry)
            __result = new ModelId(__result.Category, entry);
    }
}
