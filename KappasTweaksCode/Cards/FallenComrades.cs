using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using TargetPower = MegaCrit.Sts2.Core.Models.Powers.IntangiblePower;

namespace KappasTweaks.KappasTweaksCode.Cards;

[Pool(typeof(ColorlessCardPool))]
public class FallenComrades() : KappasTweaksCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    private const int _powerAmount = 0;
    private const int _powerAmountIncreaseBy = 1;
    private const int _energyUpgrade = 1;
    private const string _powerAmountKey = "PowerAmount";
    private const string _powerAmountIncreaseByKey = "PowerAmountIncreaseBy";
    private const string _powerKey = "PowerName";
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<IntangiblePower>(_powerAmountKey, _powerAmount),
        new IntVar(_powerAmountIncreaseByKey, _powerAmountIncreaseBy),
        new StringVar(_powerKey, ModelDb.Power<TargetPower>().Title.GetFormattedText()),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TargetPower>()];
    
    public override Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        CardModel? source,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (wasRemovalPrevented)
            return Task.CompletedTask;
        if (Pile is not { IsCombatPile: true })
            return Task.CompletedTask;
        DynamicVars[_powerAmountKey].UpgradeValueBy(DynamicVars[_powerAmountIncreaseByKey].IntValue);
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        TargetPower? intangiblePower = await PowerCmd.Apply<TargetPower>(choiceContext, Owner.Creature, DynamicVars[_powerAmountKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-_energyUpgrade);
    }
}