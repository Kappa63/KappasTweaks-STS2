using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class DominoAvalanche() : BonesmanCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 24;
    private const int _damageUpgrade = 8;
    private const int _bonus = 12;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [BonesmanKeywords.Jumbled];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new IntVar(BonusKey, _bonus),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue + (chained ? Bonus : 0));
        if (chained)
            (await Chain.Ensure(choiceContext, Owner)).SetEnds(ChainPower.Blank, ChainPower.Blank);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
