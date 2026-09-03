namespace Wizardz.Shared.Models;

public class HeroEntity
{
    public double WorldX { get; set; } = 1000.0;
    public double WorldY { get; set; } = 1000.0;
    public double VelocityX { get; set; } = 0.0;
    public double VelocityY { get; set; } = 0.0;
    public double FacingAngle { get; set; } = 0.0; // Degrees

    public double CurrentHealth { get; set; } = 100.0;
    public double MaxHealth { get; set; } = 100.0;
    public double MoveSpeed { get; set; } = 180.0; // World units per second
    public double MagnetRadius { get; set; } = 120.0; // Pickup range for gems
    public double InvulnerabilityTimer { get; set; } = 0.0;

    // Animation recoil
    public bool IsAttacking { get; set; } = false;
    public double AttackAnimationTimer { get; set; } = 0.0;

    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvulnerable => InvulnerabilityTimer > 0;
}

public class MonsterEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Slime";
    public string Icon { get; set; } = "🟢";
    public double WorldX { get; set; }
    public double WorldY { get; set; }
    public double Speed { get; set; } = 75.0; // Homing speed
    public double MaxHealth { get; set; } = 30.0;
    public double CurrentHealth { get; set; } = 30.0;
    public double Damage { get; set; } = 10.0; // Contact damage
    public double XpReward { get; set; } = 10.0;
    public double ManaReward { get; set; } = 15.0;
    public bool IsBoss { get; set; } = false;
    public bool IsHit { get; set; } = false;

    public bool IsDead => CurrentHealth <= 0;
}

public class XpGemEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public double WorldX { get; set; }
    public double WorldY { get; set; }
    public double Value { get; set; } = 10.0;
    public string Icon => Value >= 50 ? "💎" : "💠";
    public bool IsMagnetized { get; set; } = false;
}

public class SpellProjectile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public double WorldX { get; set; }
    public double WorldY { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Damage { get; set; }
    public bool IsCrit { get; set; }
    public SpellElement Element { get; set; }
    public string Icon { get; set; } = "🔥";
    public double Speed { get; set; } = 380.0;
    public double Lifetime { get; set; } = 2.0;
    public int PierceRemaining { get; set; } = 0;
    public double AreaRadius { get; set; } = 35.0;
    public int ChainRemaining { get; set; } = 0;
    public string? TargetMonsterId { get; set; }
}

public class ImpactVFX
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public double WorldX { get; set; }
    public double WorldY { get; set; }
    public string Icon { get; set; } = "💥";
    public double Age { get; set; } = 0.0;
    public double MaxAge { get; set; } = 0.35;
    public SpellElement Element { get; set; }
}

public class DungeonStairs
{
    public double WorldX { get; set; } = 1000.0;
    public double WorldY { get; set; } = 900.0;
    public bool IsActive { get; set; } = false;
}
