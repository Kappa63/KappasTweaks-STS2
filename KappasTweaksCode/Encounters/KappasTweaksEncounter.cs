using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using KappasTweaks.KappasTweaksCode.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace KappasTweaks.KappasTweaksCode.Encounters;

public abstract class KappasTweaksEncounter(RoomType roomType) : CustomEncounterModel(roomType)
{
    private const string _dynamicBackgroundScene = "res://BaseLib/scenes/dynamic_background.tscn";

    protected virtual string AssetName => Id.Entry.RemovePrefix().ToLowerInvariant();

    private string MapIcon => $"{AssetName}/map_icon";

    private string OutlinePath => $"{MapIcon}_outline.png".EncounterImagePath();

    private string RoomScenePath => $"rooms/background_scenes/{AssetName}.tscn".ScenePath();

    // The map node appends ".png" and "_outline.png" itself, so this one stays extensionless.
    public override string BossNodePath => MapIcon.EncounterImagePath();

    public override string CustomRunHistoryIconPath => $"{MapIcon}.png".EncounterImagePath();

    public override string CustomRunHistoryIconOutlinePath =>
        ResourceLoader.Exists(OutlinePath) ? OutlinePath : CustomRunHistoryIconPath;

    public override BackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        if (!ResourceLoader.Exists(RoomScenePath))
            return null;

        try
        {
            return new CustomBackgroundAssets(_dynamicBackgroundScene, new[] { new[] { RoomScenePath } },
                Array.Empty<string>(), rng);
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"{Id.Entry} room unavailable, using act background: {e.Message}");
            return null;
        }
    }
}
