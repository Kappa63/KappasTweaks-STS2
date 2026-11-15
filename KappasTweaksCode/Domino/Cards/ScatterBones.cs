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

// Took over Clatter's art: the picture already shows bones flying everywhere.
[Pool(typeof(BonesmanCardPool))]
public sealed class ScatterBones() : BonesmanCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    // HYPER
    private const int _chips = 2;
    private const int _chipsUpgrade = 1;

    protected override bool IsDomino => false;
    protected override string ArtName => "clatter";
    protected override IEnumerable<IHoverTip> CardHoverTips => [HoverTipFactory.FromCard<BoneChip>()];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new CardsVar(_chips),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await AddChips(DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(_chipsUpgrade);
    }
}
