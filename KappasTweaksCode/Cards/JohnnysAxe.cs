using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Cards;

[Pool(typeof(ColorlessCardPool))]
public class JohnnysAxe() : KappasTweaksCard(0,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 20;
    private const int _damageUpgrade = 10;
    private const int _cardsToDraw = 1;
    private const decimal _axePercent = 50m;
    private const decimal _axePercentUpgrade = 0m;
    private const string _axeChanceKey = "AxeChance";
    private const string _curseNameKey = "CurseName";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new CardsVar(_cardsToDraw),
        new (_axeChanceKey, _axePercent),
        new StringVar(_curseNameKey, ModelDb.Card<HeresJohnny>().Title),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<HeresJohnny>()];

    private decimal AxeChance => DynamicVars[_axeChanceKey].BaseValue / 100m;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NThinSliceVfx.Create(play.Target));

        float attackerAnimDelay = Owner.Character.AttackAnimDelay;
        if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Normal)
            attackerAnimDelay += 0.2f;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithAttackerAnim("Attack", attackerAnimDelay)
            .Execute(choiceContext);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        if ((decimal)Owner.RunState.Rng.CombatCardGeneration.NextDouble() < AxeChance)
        {
            // await CardPileCmd.AddCurseToDeck<HeresJohnny>(Owner);
            // foreach (Creature creature in CombatState!.GetTeammatesOf(Owner.Creature)
            //              .Where(c => c is { IsAlive: true, IsPlayer: true } && c != Owner.Creature))
            //     await CardPileCmd.AddCurseToDeck<HeresJohnny>(creature.Player!);
            
            // JohnnysAxe axe = this.Owner.RunState.CreateCard<JohnnysAxe>(this.Owner);
            // CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(axe, PileType.Deck), 2f);
        }
        else
            await CardPileCmd.AddCurseToDeck<HeresJohnny>(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_damageUpgrade);
        // this.DynamicVars[_axeChanceKey].UpgradeValueBy(_axePercentUpgrade);
    }
}
