using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

[Pool(typeof(BonesmanCardPool))]
public sealed class Boxcars() : BonesmanCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 9;
    private const int _damageUpgrade = 3;
    private const int _cards = 2;
    private const int _energy = 1;

    protected override (int Left, int Right)? FixedPips => (6, 6);

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new CardsVar(_cards),
        new EnergyVar(_energy),
    ];

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue, 2);
        if (chained)
        {
            await Draw(choiceContext, DynamicVars.Cards.BaseValue);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
        (await Chain.Ensure(choiceContext, Owner)).SetEnds(ChainPower.MaxPip, ChainPower.MaxPip);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
    }
}
