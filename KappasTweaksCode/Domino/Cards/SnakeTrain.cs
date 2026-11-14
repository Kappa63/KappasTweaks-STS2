using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class SnakeTrain() : BonesmanCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    // HYPER
    private const int _damage = 5;
    private const int _damageUpgrade = 2;

    protected override (int Left, int Right)? FixedPips => (1, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        int hits = Math.Max(1, Chain.Of(Owner)?.Links ?? 0);
        for (int i = 0; i < hits; i++)
            await HitAll(choiceContext, play, DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
