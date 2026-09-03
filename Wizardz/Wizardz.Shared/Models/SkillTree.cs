namespace Wizardz.Shared.Models;

public enum SkillBranch
{
    Pyromancy,      // The Phoenix
    Electromancy,   // The Thunder Drake
    Cryomancy,      // The Frost Serpent
    ArcaneMastery,  // The Cosmic Eye
    Vitality        // The Iron Colossus
}

public class SkillNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "✨";
    public SkillBranch Branch { get; set; }
    public int Level { get; set; } = 0;
    public int MaxLevel { get; set; } = 5;
    public double BaseCost { get; set; } = 50.0;
    public double CostMultiplier { get; set; } = 1.6;
    public string? PrerequisiteNodeId { get; set; }

    // Spell & Stat Attributes
    public bool IsSpellUnlock { get; set; } = false;
    public string? AssociatedSpellId { get; set; }
    public double StatBonusPerLevel { get; set; } = 0;

    // Constellation Map 2D Coordinates (0 to 1000 scale)
    public double ConstellationX { get; set; } = 500;
    public double ConstellationY { get; set; } = 500;
    public bool IsMajorStar { get; set; } = false; // Keystones vs minor attribute stars

    public double GetCostForNextLevel()
    {
        if (Level >= MaxLevel) return double.PositiveInfinity;
        return Math.Round(BaseCost * Math.Pow(CostMultiplier, Level));
    }

    public bool IsUnlocked => Level > 0;
}

public class ConstellationLine
{
    public string FromNodeId { get; set; } = string.Empty;
    public string ToNodeId { get; set; } = string.Empty;
    public SkillBranch Branch { get; set; }
}
