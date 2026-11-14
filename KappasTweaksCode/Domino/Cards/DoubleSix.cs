using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class DoubleSix() : BonesmanCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 5;
    private const int _damageUpgrade = 2;
    private const int _hits = 2;
    private const int _cards = 1;

    protected override (int Left, int Right)? FixedPips => (6, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new CardsVar(_cards),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue, _hits);
        if (chained)
            await Draw(choiceContext, DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
