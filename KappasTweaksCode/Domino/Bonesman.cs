using BaseLib.Abstracts;
using Godot;
using KappasTweaks.KappasTweaksCode.Domino.Cards;
using KappasTweaks.KappasTweaksCode.Domino.Relics;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Borrows the Necrobinder's body, map marker, energy orb, rest site and sounds until he has his own art.
public sealed class Bonesman : PlaceholderCharacterModel
{
    // HYPER
    private const int _startingHp = 72;
    private const int _startingGold = 99;
    private const int _strikes = 5;
    private const int _defends = 4;

    public override string PlaceholderID => "necrobinder";

    public override Color NameColor => new("E8DCB8");
    public override CharacterGender Gender => CharacterGender.Masculine;
    protected override CharacterModel? UnlocksAfterRunAs => null;
    public override int StartingHp => _startingHp;
    public override int StartingGold => _startingGold;
    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.4f;

    public override CardPoolModel CardPool => ModelDb.CardPool<BonesmanCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<BonesmanRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<BonesmanPotionPool>();

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ..Enumerable.Repeat<CardModel>(ModelDb.Card<StrikeBonesman>(), _strikes),
        ..Enumerable.Repeat<CardModel>(ModelDb.Card<DefendBonesman>(), _defends),
        ModelDb.Card<DoubleSix>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<BoneBox>()];
}
