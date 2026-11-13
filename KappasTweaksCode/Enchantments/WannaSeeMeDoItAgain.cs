using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KappasTweaks.KappasTweaksCode.Enchantments;

public sealed class WannaSeeMeDoItAgain : KappasTweaksEnchantment
{
    // HYPER
    private const int _playsRequired = 5;
    private const int _replayCount = 5;
    private const string _playsKey = "Plays";
    private const string _timesKey = "Times";

    private int _timesPlayedThisCombat;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(_playsKey, _playsRequired),
        new IntVar(_timesKey, _replayCount),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, DynamicVars[_timesKey]),
    ];

    private int PlaysRequired => DynamicVars[_playsKey].IntValue;

    public override bool ShowAmount => true;

    public override int DisplayAmount => TimesPlayedThisCombat;

    private int TimesPlayedThisCombat
    {
        get => _timesPlayedThisCombat;
        set
        {
            AssertMutable();
            _timesPlayedThisCombat = value;
        }
    }

    public override int EnchantPlayCount(int originalPlayCount)
    {
        return TimesPlayedThisCombat >= PlaysRequired
            ? originalPlayCount + DynamicVars[_timesKey].IntValue
            : originalPlayCount;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != Card || TimesPlayedThisCombat >= PlaysRequired)
            return Task.CompletedTask;
        TimesPlayedThisCombat++;
        return Task.CompletedTask;
    }
}
