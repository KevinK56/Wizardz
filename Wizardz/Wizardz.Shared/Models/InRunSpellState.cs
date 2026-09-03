namespace Wizardz.Shared.Models;

public enum SpellElement
{
    Fire,
    Lightning,
    Frost,
    Arcane
}

public class InRunSpellState
{
    public string SpellId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "✨";
    public SpellElement Element { get; set; } = SpellElement.Arcane;

    public int Level { get; set; } = 1;
    public int MaxLevel { get; set; } = 5;

    public double BaseCooldown { get; set; } = 1.5;
    public double CooldownRemaining { get; set; } = 0.0;
    public double BaseDamage { get; set; } = 15.0;
    public double DamagePerLevel { get; set; } = 8.0;
    public int ProjectileCount { get; set; } = 1;
    public double AreaRadius { get; set; } = 40.0;
    public int ChainCount { get; set; } = 0; // For lightning

    public double CurrentDamage => BaseDamage + (Level - 1) * DamagePerLevel;
    public double CurrentCooldown => Math.Max(0.3, BaseCooldown * Math.Pow(0.92, Level - 1));

    public string GetUpgradeSummary()
    {
        if (Level >= MaxLevel) return "MAX LEVEL";
        int next = Level + 1;
        double nextDmg = BaseDamage + (next - 1) * DamagePerLevel;
        return $"+{DamagePerLevel} DMG (Total: {nextDmg:0.#})";
    }
}

public class DraftOption
{
    public string SpellId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "✨";
    public string RarityLabel { get; set; } = "UPGRADE";
    public int CurrentLevel { get; set; }
    public int TargetLevel { get; set; }
    public bool IsNewUnlock { get; set; }
    public SpellElement Element { get; set; } = SpellElement.Arcane;
}
