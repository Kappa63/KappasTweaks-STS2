using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KappasTweaks.KappasTweaksCode.Ancients;

public class KappaTheAncient : KappasTweaksAncient
{
    public override bool IsValidForAct(ActModel act)
    {
        return false;
    }

    protected override OptionPools MakeOptionPools => new OptionPools(
        [
            AncientOption<GottaCatchEmAll>(),
            AncientOption<HahaBomba>(),
            AncientOption<TheShining>(),
            AncientOption<FearTheReaper>(),
            AncientOption<FirstBlood>(),
        ]
    );
}
