using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Relics;
using KappasTweaks.KappasTweaksCode.RestSiteOptions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;

namespace KappasTweaks.KappasTweaksCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class FusionGate() : KappasTweaksRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override bool IsAllowedInternal(IRunState runState)
    {
        return !IsLateInAct3(runState);
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
            return false;
        options.Add(new FuseRestSiteOption(player));
        return true;
    }
}