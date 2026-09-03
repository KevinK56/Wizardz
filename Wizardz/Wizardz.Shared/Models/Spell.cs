namespace Wizardz.Shared.Models;

public enum SpellEffectType
{
    ArcaneSurge,    // Multiplies click and MPS for a limited duration
    TimeWarp,       // Instantly grants N seconds of production
    Transmutation   // Converts mana into essence
}

public class Spell
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "⚡";
    public double ManaCost { get; set; }
    public double EssenceCost { get; set; }
    public double CooldownSeconds { get; set; }
    public double DurationSeconds { get; set; }
    public SpellEffectType EffectType { get; set; }
    public double PowerMultiplier { get; set; } = 1.0;

    public double CurrentCooldownRemaining { get; set; }
    public double CurrentDurationRemaining { get; set; }

    public bool IsActive => CurrentDurationRemaining > 0;
    public bool IsOnCooldown => CurrentCooldownRemaining > 0;

    public bool CanCast(double mana, double essence)
    {
        return !IsActive && !IsOnCooldown && mana >= ManaCost && essence >= EssenceCost;
    }

    public void Tick(double deltaSeconds)
    {
        if (CurrentDurationRemaining > 0)
        {
            CurrentDurationRemaining = Math.Max(0, CurrentDurationRemaining - deltaSeconds);
        }

        if (CurrentCooldownRemaining > 0)
        {
            CurrentCooldownRemaining = Math.Max(0, CurrentCooldownRemaining - deltaSeconds);
        }
    }
}
