namespace Wizardz.Shared.Models;

public class DungeonMonster
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "👾";
    public double MaxHealth { get; set; }
    public double CurrentHealth { get; set; }
    public double X { get; set; } // Percentage 0 - 100 within room
    public double Y { get; set; }
    public double Speed { get; set; } = 12.0; // % per second towards hero
    public double ManaReward { get; set; }
    public double EssenceReward { get; set; }
    public bool IsBoss { get; set; } = false;
    public bool IsHit { get; set; } = false;

    public bool IsDead => CurrentHealth <= 0;
}

public class MagicProjectile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double TargetX { get; set; }
    public double TargetY { get; set; }
    public double CurrentX { get; set; }
    public double CurrentY { get; set; }
    public double Speed { get; set; } = 150.0; // % distance per second
    public double Progress { get; set; } = 0.0; // 0.0 to 1.0
    public double Damage { get; set; }
    public bool IsCrit { get; set; }
    public string TargetMonsterId { get; set; } = string.Empty;
    public string Icon { get; set; } = "✨";
}
