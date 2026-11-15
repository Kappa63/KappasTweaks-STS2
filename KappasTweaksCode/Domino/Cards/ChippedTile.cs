using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

// Dingy Dagger for dominoes: upgrades forever, renamed and repainted at +3 and +6. Blank pips.
[Pool(typeof(BonesmanCardPool))]
public sealed class ChippedTile() : BonesmanCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    // HYPER
    private const int _polishedLevel = 3;
    private const int _gildedLevel = 6;
    private const int _damage = 7;
    private const int _damageUpgrade = 3;
    private const int _cards = 1;

    private string Stage => CurrentUpgradeLevel >= _gildedLevel ? "gilded"
        : CurrentUpgradeLevel >= _polishedLevel ? "polished" : "chipped";

    public override int MaxUpgradeLevel => 999999999;

    public override string Title
    {
        get
        {
            string title = new LocString("cards", $"{Id.Entry}.title.{Stage}").GetFormattedText();
            return CurrentUpgradeLevel > 0 ? $"{title}+{CurrentUpgradeLevel}" : title;
        }
    }

    protected override string ArtName => $"{Stage}_tile";

    protected override (int Left, int Right)? FixedPips => (0, 0);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue);
        if (chained)
            await Draw(choiceContext, DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade + CurrentUpgradeLevel);
    }
}
