using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

// Boneyard payoff: grows with every Bone Chip played this combat.
[Pool(typeof(BonesmanCardPool))]
public sealed class PipStorm() : BonesmanCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    // HYPER
    private const int _damage = 5;
    private const int _perChip = 2;
    private const int _perChipUpgrade = 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [BonesmanKeywords.Jumbled];
    protected override IEnumerable<IHoverTip> CardHoverTips => [HoverTipFactory.FromCard<BoneChip>()];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CalculationBaseVar(_damage),
        new ExtraDamageVar(_perChip),
        ScaledDamage(c => c.ChipsPlayed),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await HitAllScaled(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(_perChipUpgrade);
    }
}
