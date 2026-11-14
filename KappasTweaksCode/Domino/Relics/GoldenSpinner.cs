using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Domino.Cards;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using KappasTweaks.KappasTweaksCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Relics;

[Pool(typeof(BonesmanRelicPool))]
public sealed class GoldenSpinner : KappasTweaksRelic, IChainListener
{
    // HYPER
    private const int _energy = 1;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task OnChained(PlayerChoiceContext context, BonesmanCard card)
    {
        if (!card.IsDouble)
            return;
        Flash();
        await PlayerCmd.GainEnergy(_energy, Owner);
    }
}
