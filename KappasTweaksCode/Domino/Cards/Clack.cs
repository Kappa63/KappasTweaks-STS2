using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Clack() : BonesmanCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 8;
    private const int _damageUpgrade = 3;
    private const int _block = 5;

    protected override (int Left, int Right)? FixedPips => (4, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new BlockVar(_block, ValueProp.Move),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue);
        if (chained)
            await Block(play, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
