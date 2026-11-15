using BaseLib.Utils;
using KappasTweaks.KappasTweaksCode.Domino.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace KappasTweaks.KappasTweaksCode.Domino.Cards;

// The Boneyard token: cheap, Jumbled, gone after one use. Every chip is one more domino to Chain with.
[Pool(typeof(TokenCardPool))]
public sealed class BoneChip() : BonesmanCard(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    // HYPER
    private const int _damage = 3;
    private const int _block = 3;
    private const int _upgrade = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [BonesmanKeywords.Jumbled, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CardVars =>
    [
        new DamageVar(_damage, ValueProp.Move),
        new BlockVar(_block, ValueProp.Move),
    ];

    public static async Task<IEnumerable<CardModel>> CreateInHand(MegaCrit.Sts2.Core.Entities.Players.Player owner, int count, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
    {
        if (count <= 0 || MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsOverOrEnding)
            return [];
        List<CardModel> chips = [];
        for (int i = 0; i < count; i++)
            chips.Add(combatState.CreateCard<BoneChip>(owner));
        await CardPileCmd.AddGeneratedCardsToCombat(chips, PileType.Hand, owner);
        return chips;
    }

    protected override async Task OnDominoPlay(PlayerChoiceContext choiceContext, CardPlay play, bool chained)
    {
        Chain.Of(Owner)?.ChipPlayed();
        await Hit(choiceContext, play, DynamicVars.Damage.BaseValue);
        if (chained)
            await Block(play, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_upgrade);
        DynamicVars.Block.UpgradeValueBy(_upgrade);
    }
}
