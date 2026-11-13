using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace KappasTweaks.KappasTweaksCode.Relics;

[Pool(typeof(SharedRelicPool))]
public sealed class TheShining : KappasTweaksRelic
{
    // HYPER
    private const int _axesToAdd = 1;
    private const int _cursesToAdd = 1;
    private const string _axeCountKey = "Axes";
    private const string _curseCountKey = "Curses";
    private const string _axeNameKey = "AxeName";
    private const string _curseNameKey = "CurseName";

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(_axeCountKey, _axesToAdd),
        new IntVar(_curseCountKey, _cursesToAdd),
        new StringVar(_axeNameKey, ModelDb.Card<JohnnysAxe>().Title),
        new StringVar(_curseNameKey, ModelDb.Card<HeresJohnny>().Title),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<JohnnysAxe>(),
        HoverTipFactory.FromCard<HeresJohnny>(),
    ];

    public override async Task AfterObtained()
    {
        for (int i = 0; i < this.DynamicVars[_axeCountKey].IntValue; i++)
        {
            JohnnysAxe axe = this.Owner.RunState.CreateCard<JohnnysAxe>(this.Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(axe, PileType.Deck), 2f);
        }

        for (int i = 0; i < this.DynamicVars[_curseCountKey].IntValue; i++)
            await CardPileCmd.AddCurseToDeck<HeresJohnny>(this.Owner);
    }
}
