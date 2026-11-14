using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class LongChain() : BonesmanCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 8;
    private const int _damageUpgrade = 3;
    private const int _perLink = 3;
    private const int _perLinkUpgrade = 1;

    protected override (int Left, int Right)? FixedPips => (6, 1);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new IntVar(BonusKey, _perLink),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue + Bonus * (Chain.Of(Owner)?.Links ?? 0));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
        DynamicVars[BonusKey].UpgradeValueBy(_perLinkUpgrade);
    }
}
