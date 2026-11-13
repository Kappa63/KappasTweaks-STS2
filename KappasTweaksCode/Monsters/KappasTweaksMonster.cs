using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils.NodeFactories;
using Godot;
using KappasTweaks.KappasTweaksCode.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KappasTweaks.KappasTweaksCode.Monsters;

public abstract class KappasTweaksMonster : CustomMonsterModel
{
    private string? _spriteState;

    protected string AssetName => Id.Entry.RemovePrefix().ToLowerInvariant();

    protected abstract string DefaultSpriteState { get; }

    protected virtual IEnumerable<string> SpriteStates => [];

    protected virtual float VisualScale => 1f;

    public override string CustomVisualPath => SpritePath(DefaultSpriteState);

    public override IEnumerable<string> AssetPaths =>
        SpriteStates.Prepend(DefaultSpriteState).Distinct().Select(SpritePath);

    public override NCreatureVisuals CreateCustomVisuals()
    {
        NCreatureVisuals visuals = NodeFactory<NCreatureVisuals>.CreateFromResource(SpritePath(_spriteState ?? DefaultSpriteState));
        visuals.DefaultScale = VisualScale;
        visuals.Scale = Vector2.One * VisualScale;
        return visuals;
    }

    protected string? SpriteState
    {
        get => _spriteState;
        set
        {
            AssertMutable();
            _spriteState = value;
            SwapSprite();
        }
    }

    private string SpritePath(string state)
    {
        string path = $"{AssetName}/{state}.png".MonsterImagePath();

        if (!ResourceLoader.Exists(path))
            MainFile.Logger.Info($"Missing monster sprite: {path}");

        return path;
    }

    private void SwapSprite()
    {
        NCreature? node = NCombatRoom.Instance?.CreatureNodes
            .FirstOrDefault(c => ReferenceEquals(c.Entity.Monster, this));

        if (node?.Visuals.GetCurrentBody() is Sprite2D sprite)
            sprite.Texture = PreloadManager.Cache.GetTexture2D(SpritePath(_spriteState ?? DefaultSpriteState));
    }
}
