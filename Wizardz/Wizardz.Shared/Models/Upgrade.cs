namespace Wizardz.Shared.Models;

public enum UpgradeTargetType
{
    SpecificUnit,
    ClickPower,
    GlobalMps,
    PrestigeAstral
}

public class Upgrade
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "✨";
    public double CostMana { get; set; }
    public double CostEssence { get; set; }
    public double CostAstralShards { get; set; }
    public UpgradeTargetType TargetType { get; set; } = UpgradeTargetType.SpecificUnit;
    public string TargetUnitId { get; set; } = string.Empty;
    public double Multiplier { get; set; } = 2.0;
    public bool IsPurchased { get; set; }
    public double RequiredLifetimeMana { get; set; }

    public bool CanAfford(double mana, double essence, double astralShards)
    {
        if (IsPurchased) return false;
        return mana >= CostMana && essence >= CostEssence && astralShards >= CostAstralShards;
    }
}
