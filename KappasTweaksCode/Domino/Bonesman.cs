using BaseLib.Abstracts;
using Godot;
using KappasTweaks.KappasTweaksCode.Domino.Cards;
using KappasTweaks.KappasTweaksCode.Domino.Relics;
using KappasTweaks.KappasTweaksCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Each visual uses his own file under images/character/ once it exists, otherwise the Necrobinder's. The body, campfire
// and merchant poses are plain PNGs; BaseLib turns them into the game's nodes.
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

    public override CustomEnergyCounter? CustomEnergyCounter =>
        new(layer => $"character/energy/layer{layer}.png".ImagePath(), new Color("5A4632"), new Color("FFD68C"));
    public override Color EnergyLabelOutlineColor => new("5A4632");

    public override string CustomVisualPath => Own("bonesman.png") ?? base.CustomVisualPath;
    public override string CustomRestSiteAnimPath => Own("bonesman_rest.png") ?? base.CustomRestSiteAnimPath;
    public override string CustomMerchantAnimPath => Own("bonesman.png") ?? base.CustomMerchantAnimPath;
    public override string? CustomCharacterSelectIconPath => Own("select.png") ?? base.CustomCharacterSelectIconPath;
    public override string? CustomCharacterSelectLockedIconPath => Own("select_locked.png") ?? base.CustomCharacterSelectLockedIconPath;
    public override string? CustomIconTexturePath => Own("icon.png") ?? base.CustomIconTexturePath;
    public override string? CustomIconOutlineTexturePath => Own("icon_outline.png") ?? base.CustomIconOutlineTexturePath;
    public override string? CustomMapMarkerPath => Own("map_marker.png") ?? base.CustomMapMarkerPath;

    public override string CustomCharacterSelectBg =>
        ResourceLoader.Exists(_selectBgScene) ? _selectBgScene : base.CustomCharacterSelectBg;

    // The small icon used in lists (run history, multiplayer): vanilla's is just a TextureRect filling its slot.
    public override Control? CustomIcon => Own("icon.png") is { } icon
        ? new TextureRect
        {
            Texture = ResourceLoader.Load<Texture2D>(icon),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorRight = 1f,
            AnchorBottom = 1f,
        }
        : null;

    private static readonly string _selectBgScene = "character/select_bg.tscn".ScenePath();

    private static string? Own(string file)
    {
        string path = $"character/{file}".ImagePath();
        return ResourceLoader.Exists(path) ? path : null;
    }

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
