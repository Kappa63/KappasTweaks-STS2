using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace KappasTweaks.KappasTweaksCode.Cards;

[Pool(typeof(CurseCardPool))]
public class HeresJohnny() : KappasTweaksCard(0,
    CardType.Curse, CardRarity.Curse,
    TargetType.None)
{
    /// HYPER
    private const int _energyCostIncrease = 1;
    private const string _axeNameKey = "AxeName";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(_energyCostIncrease),
        new StringVar(_axeNameKey, ModelDb.Card<JohnnysAxe>().Title),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<JohnnysAxe>()];

    public override int MaxUpgradeLevel => 0;
    protected override bool ShouldGlowRedInternal => true;

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || IsClone)
            return Task.CompletedTask;
    
        int axePlaysThisCombat = CombatManager.Instance.History.CardPlaysFinished
            .Count(e => e.CardPlay.Card.Owner == Owner && e.CardPlay.Card is JohnnysAxe);
        
        EnergyCost.AddThisCombat(axePlaysThisCombat * DynamicVars.Energy.IntValue);
        return Task.CompletedTask;
    }
    
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card is not JohnnysAxe)
            return Task.CompletedTask;
        EnergyCost.AddThisCombat(DynamicVars.Energy.IntValue);
        return Task.CompletedTask;
    }
    
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card.Owner != Owner)
            return true;
    
        CardPile? pile = Pile;
        if (pile == null || pile.Type != PileType.Hand)
            return true;
    
        if (card is HeresJohnny)
            return true;
    
        return autoPlayType != AutoPlayType.None;
    }
}