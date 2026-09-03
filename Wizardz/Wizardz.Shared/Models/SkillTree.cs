namespace Wizardz.Shared.Models;

public enum SkillBranch
{
    Pyromancy,      // Fire: explosive damage, burns, meteor
    Electromancy,   // Lightning: chain arcs, speed, stun
    Cryomancy,      // Frost: piercing shards, freeze, blizzard
    ArcaneMastery,  // Arcane: homing darts, spell echo, cooldown
    Vitality        // Hero: max HP, move speed, magnet radius
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

    public double GetCostForNextLevel()
    {
        if (Level >= MaxLevel) return double.PositiveInfinity;
        return Math.Round(BaseCost * Math.Pow(CostMultiplier, Level));
    }

    public bool IsUnlocked => Level > 0;
}
