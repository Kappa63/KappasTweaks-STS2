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

// Took over Shuffle Up's art (tiles spinning between hands).
[Pool(typeof(BonesmanCardPool))]
public sealed class MirrorMatch() : BonesmanCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override bool IsDomino => false;
    protected override string ArtName => "shuffle_up";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        foreach (BonesmanCard card in Owner.PlayerCombatState!.Hand.Cards.OfType<BonesmanCard>().Where(c => c.Domino && c.PipLeft >= 0).ToList())
            card.MakeDouble();
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
