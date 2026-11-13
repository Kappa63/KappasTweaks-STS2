using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Relics;

[Pool(typeof(SharedRelicPool))]
public sealed class PerfectCounter : KappasTweaksRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    
    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer == null || !props.IsPoweredAttack() && !(cardSource is Omnislice))
            return;
        if (Owner.Creature.Block != amount && dealer.IsEnemy)
            return;
        Flash();
        IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, dealer, amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, target, (CardModel) null, (CardPlay) null);
    }
}
