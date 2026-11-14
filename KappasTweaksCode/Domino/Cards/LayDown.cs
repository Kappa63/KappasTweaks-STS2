using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class LayDown() : BonesmanCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    // HYPER
    private const int _block = 6;
    private const int _blockUpgrade = 3;
    private const int _cards = 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [BonesmanKeywords.Jumbled];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new BlockVar(_block, ValueProp.Move),
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Block(play, DynamicVars.Block.BaseValue);
        await Draw(choiceContext, DynamicVars.Cards.BaseValue + (chained ? DynamicVars.Cards.BaseValue : 0));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(_blockUpgrade);
    }
}
