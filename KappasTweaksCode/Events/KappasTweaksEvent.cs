using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Events;

public abstract class KappasTweaksEvent : CustomEventModel
{
    protected string AssetName => Id.Entry.RemovePrefix().ToLowerInvariant();

    public override string CustomInitialPortraitPath => Required($"{AssetName}.png".EventImagePath());

    public override string CustomBackgroundScenePath =>
        Required($"events/background_scenes/{AssetName}.tscn".ScenePath());

    private static string Required(string path)
    {
        if (!ResourceLoader.Exists(path))
            MainFile.Logger.Info($"Missing required event asset: {path}");
        return path;
    }
}
