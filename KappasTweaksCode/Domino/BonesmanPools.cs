using BaseLib.Abstracts;
using Godot;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Pools keep BaseLib's own energy colour name, which routes the card-cost gem and in-text energy icon to these files.
internal static class BonesmanEnergy
{
    public static readonly string BigIcon = "character/energy_icon_big.png".ImagePath();
    public static readonly string TextIcon = "character/energy_icon_text.png".ImagePath();
}

public sealed class BonesmanCardPool : CustomCardPoolModel
{
    public override string Title => "bonesman";
    public override Color ShaderColor => new("D9CBA3");
    public override Color DeckEntryCardColor => new("D9CBA3");
    public override bool IsColorless => false;
    public override string? BigEnergyIconPath => BonesmanEnergy.BigIcon;
    public override string? TextEnergyIconPath => BonesmanEnergy.TextIcon;
}

public sealed class BonesmanRelicPool : CustomRelicPoolModel
{
    public override string? BigEnergyIconPath => BonesmanEnergy.BigIcon;
    public override string? TextEnergyIconPath => BonesmanEnergy.TextIcon;
}

public sealed class BonesmanPotionPool : CustomPotionPoolModel
{
    public override string? BigEnergyIconPath => BonesmanEnergy.BigIcon;
    public override string? TextEnergyIconPath => BonesmanEnergy.TextIcon;
}
