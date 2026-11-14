using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Breaker() : BonesmanCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 12;
    private const int _damageUpgrade = 4;
    private const int _bonus = 6;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [BonesmanKeywords.Jumbled];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new IntVar(BonusKey, _bonus),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue + (Outcome == ChainOutcome.Broke ? Bonus : 0));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
