using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Relics;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace KappasTweaks.KappasTweaksCode.Domino.Relics;

// Blank ends match anything, so the first domino of every combat Chains.
[Pool(typeof(BonesmanRelicPool))]
public sealed class BoneBox : KappasTweaksRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(BonesmanKeywords.Chained)];

    public override async Task BeforeCombatStart()
    {
        Flash();
        (await Chain.Ensure(new BlockingPlayerChoiceContext(), Owner)).SetEnds(ChainPower.Blank, ChainPower.Blank);
    }
}
