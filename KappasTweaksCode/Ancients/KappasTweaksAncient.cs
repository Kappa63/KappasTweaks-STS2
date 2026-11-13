using BaseLib.Abstracts;
using BaseLib.Extensions;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Ancients;

public abstract class KappasTweaksAncient : CustomAncientModel
{
    protected string AssetName => Id.Entry.RemovePrefix().ToLowerInvariant();

    public override string CustomScenePath => $"events/background_scenes/{AssetName}.tscn".ScenePath();

    public override string CustomMapIconPath => $"ancient_node_{AssetName}.png".AncientMapIconPath();

    public override string CustomMapIconOutlinePath => $"ancient_node_{AssetName}_outline.png".AncientMapIconPath();

    public override string CustomRunHistoryIconPath => $"{AssetName}.png".RunHistoryImagePath();

    public override string CustomRunHistoryIconOutlinePath => $"{AssetName}_outline.png".RunHistoryImagePath();
}
