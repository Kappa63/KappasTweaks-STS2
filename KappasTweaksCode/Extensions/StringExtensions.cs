using Godot;

namespace KappasTweaks.KappasTweaksCode.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", path);
    }

    public static string CardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find card image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big card image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find power image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "powers", "power.png");
    }
    
    public static string RestSiteOptionImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "ui", "rest_site", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find rest site option image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "ui", "rest_site", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big power image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "relic.png");
    }
    
    // No fallback: EnchantmentModel.Icon loads this with no existence check, so pointing at a placeholder
    // that may not exist just moves the crash onto a filename that isn't the one you forgot.
    public static string EnchantmentImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "enchantments", path);
        if (!ResourceLoader.Exists(path))
            MainFile.Logger.Info("Could not find enchantment image path: " + path);

        return path;
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string PotionImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "potions", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find potion image path: " + path);
        // The mod template ships no potion placeholder, so fall back to the base game's. PotionModel.Image
        // does a bare ResourceLoader.Load with no existence check, so this must resolve to something real.
        return "res://images/packed/potions/potion_placeholder.png";
    }

    /// <summary>
    /// Potion outlines are optional and take no fallback: PotionModel.OutlinePath runs its own
    /// ResourceLoader.Exists check and returns null when the file is absent, which the game handles.
    /// </summary>
    public static string PotionOutlineImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "potions", path);
    }

    public static string CharacterUiPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "charui", path);
    }

    public static string AncientMapIconPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "map", "ancients", path);
    }

    public static string EventImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "events", path);
    }

    public static string MonsterImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "monsters", path);
    }

    public static string EncounterImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "encounters", path);
    }

    // No fallback: a missing icon resolves to no icon at all, which the player list already handles.
    public static string MultiplayerIconPath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "ui", "multiplayer", path);
        if (!ResourceLoader.Exists(path))
            MainFile.Logger.Info("Could not find multiplayer icon path: " + path);

        return path;
    }

    public static string RunHistoryImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "ui", "run_history", path);
    }

    public static string ScenePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "scenes", path);
    }
}