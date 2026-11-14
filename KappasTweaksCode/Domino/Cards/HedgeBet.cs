using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class HedgeBet() : BonesmanCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    // HYPER
    private const int _block = 6;
    private const int _blockUpgrade = 3;
    private const int _energy = 1;

    protected override (int Left, int Right)? FixedPips => (4, 4);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new BlockVar(_block, ValueProp.Move),
        new EnergyVar(_energy),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Block(play, DynamicVars.Block.BaseValue);
        if (chained)
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(_blockUpgrade);
    }
}
