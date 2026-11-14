using BaseLib.Abstracts;
using Godot;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Energy icons borrow the Necrobinder's until the Bonesman has his own.
public sealed class BonesmanCardPool : CustomCardPoolModel
{
    public override string Title => "bonesman";
    public override string EnergyColorName => "necrobinder";
    public override Color ShaderColor => new("D9CBA3");
    public override Color DeckEntryCardColor => new("D9CBA3");
    public override bool IsColorless => false;
}

public sealed class BonesmanRelicPool : CustomRelicPoolModel
{
    public override string EnergyColorName => "necrobinder";
}

public sealed class BonesmanPotionPool : CustomPotionPoolModel
{
    public override string EnergyColorName => "necrobinder";
}
