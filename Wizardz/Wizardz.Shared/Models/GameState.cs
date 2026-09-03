using System.Text.Json.Serialization;
using Wizardz.Shared.Services;

namespace Wizardz.Shared.Models;

public class GameState
{
    public double Mana { get; set; } = 0;
    public double ArcaneEssence { get; set; } = 0;
    public double AstralShards { get; set; } = 0;
    public double LifetimeMana { get; set; } = 0;
    public long TotalClicks { get; set; } = 0;
    public int AscensionCount { get; set; } = 0;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSaveTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastTickTimeUtc { get; set; } = DateTime.UtcNow;

    // User Preferences & Settings
    public int SelectedBuyQuantity { get; set; } = 1;
    public bool IsBuyMaxSelected { get; set; } = false;
    public int LocalAutoSaveIntervalMinutes { get; set; } = 2; // Default 2 minutes
    public int CloudAutoSaveIntervalMinutes { get; set; } = 5; // Default 5 minutes

    // Dungeon Progression & Hero Combat Stats
    public int CurrentDungeonFloor { get; set; } = 1;
    public int HighestDungeonFloor { get; set; } = 1;
    public double HeroBaseAttackPower { get; set; } = 12.0;
    public double HeroBaseAttackSpeed { get; set; } = 1.2;
    public double HeroBaseCritChance { get; set; } = 5.0;
    public double HeroBaseCritDamage { get; set; } = 150.0;

    public Dictionary<EquipmentSlot, EquipmentItem> EquippedGear { get; set; } = new();
    public List<EquipmentItem> Inventory { get; set; } = new();
    public TreasureChest? ActiveChest { get; set; }

    public List<WizardUnit> Wizards { get; set; } = new();
    public List<Upgrade> Upgrades { get; set; } = new();
    public List<Spell> Spells { get; set; } = new();

    [JsonIgnore]
    public double TotalAttackPower
    {
        get
        {
            double gearBonus = EquippedGear.Values.Sum(e => e.AttackPower);
            double globalMult = GlobalMpsMultiplier; // Tower multipliers synergize with Hero
            return (HeroBaseAttackPower + gearBonus) * globalMult;
        }
    }

    [JsonIgnore]
    public double TotalAttackSpeed
    {
        get
        {
            double speedBonusPct = EquippedGear.Values.Sum(e => e.AttackSpeedBonus);
            return HeroBaseAttackSpeed * (1.0 + (speedBonusPct / 100.0));
        }
    }

    [JsonIgnore]
    public double TotalCritChance => Math.Min(75.0, HeroBaseCritChance + EquippedGear.Values.Sum(e => e.CriticalChanceBonus));

    [JsonIgnore]
    public double TotalCritDamage => HeroBaseCritDamage + EquippedGear.Values.Sum(e => e.CriticalDamageBonus);

    [JsonIgnore]
    public double TotalManaFind => 1.0 + (EquippedGear.Values.Sum(e => e.ManaFindBonus) / 100.0);

    public void EquipItem(EquipmentItem item)
    {
        if (EquippedGear.TryGetValue(item.Slot, out var existing))
        {
            Inventory.Add(existing);
        }

        Inventory.Remove(item);
        EquippedGear[item.Slot] = item;
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (EquippedGear.TryGetValue(slot, out var item))
        {
            EquippedGear.Remove(slot);
            Inventory.Add(item);
        }
    }

    public double ScrapItem(string itemId)
    {
        var item = Inventory.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return 0;

        Inventory.Remove(item);
        double manaYield = (item.ItemLevel * 25.0) * ((int)item.Rarity + 1);
        Mana += manaYield;
        LifetimeMana += manaYield;
        return manaYield;
    }

    [JsonIgnore]
    public double EffectiveClickMultiplier
    {
        get
        {
            double mult = 1.0;
            // Upgrades affecting click
            foreach (var upg in Upgrades.Where(u => u.IsPurchased && u.TargetType == UpgradeTargetType.ClickPower))
            {
                mult *= upg.Multiplier;
            }
            // Active spells
            var surge = Spells.FirstOrDefault(s => s.EffectType == SpellEffectType.ArcaneSurge && s.IsActive);
            if (surge != null)
            {
                mult *= surge.PowerMultiplier;
            }
            // Prestige Astral shards boost (each shard gives +1% click)
            mult *= (1.0 + (AstralShards * 0.01));
            return mult;
        }
    }

    [JsonIgnore]
    public double GlobalMpsMultiplier
    {
        get
        {
            double mult = 1.0;
            // Global upgrades
            foreach (var upg in Upgrades.Where(u => u.IsPurchased && u.TargetType == UpgradeTargetType.GlobalMps))
            {
                mult *= upg.Multiplier;
            }
            // Active spells
            var surge = Spells.FirstOrDefault(s => s.EffectType == SpellEffectType.ArcaneSurge && s.IsActive);
            if (surge != null)
            {
                mult *= surge.PowerMultiplier;
            }
            // Astral Shards bonus: each shard gives +2% global MPS
            mult *= (1.0 + (AstralShards * 0.02));
            return mult;
        }
    }

    public double GetUnitMultiplier(string unitId)
    {
        double mult = 1.0;
        foreach (var upg in Upgrades.Where(u => u.IsPurchased && u.TargetType == UpgradeTargetType.SpecificUnit && u.TargetUnitId == unitId))
        {
            mult *= upg.Multiplier;
        }
        return mult;
    }

    [JsonIgnore]
    public double CurrentMps
    {
        get
        {
            double total = 0;
            double globalMult = GlobalMpsMultiplier;
            foreach (var wizard in Wizards)
            {
                total += wizard.GetTotalMps(GetUnitMultiplier(wizard.Id), globalMult);
            }
            return total;
        }
    }

    [JsonIgnore]
    public double ClickManaGain
    {
        get
        {
            // Base click is 1 + 2% of current MPS
            double baseClick = 1.0 + (CurrentMps * 0.02);
            return Math.Max(1.0, baseClick * EffectiveClickMultiplier);
        }
    }

    public double CalculateAscensionReward()
    {
        // Formula: 150 * sqrt(LifetimeMana / 1,000,000)
        if (LifetimeMana < 100_000) return 0;
        double earned = Math.Floor(150.0 * Math.Sqrt(LifetimeMana / 1_000_000.0));
        return Math.Max(0, earned);
    }

    public void PerformAscension()
    {
        double earnedShards = CalculateAscensionReward();
        if (earnedShards <= 0) return;

        AstralShards += earnedShards;
        AscensionCount++;

        // Reset currencies
        Mana = 0;
        ArcaneEssence = 0;

        // Reset wizards
        foreach (var w in Wizards)
        {
            w.Count = 0;
        }

        // Reset non-prestige upgrades
        foreach (var u in Upgrades.Where(u => u.TargetType != UpgradeTargetType.PrestigeAstral))
        {
            u.IsPurchased = false;
        }

        // Reset spell cooldowns
        foreach (var s in Spells)
        {
            s.CurrentCooldownRemaining = 0;
            s.CurrentDurationRemaining = 0;
        }

        LastTickTimeUtc = DateTime.UtcNow;
    }

    public static GameState CreateDefault()
    {
        var state = new GameState
        {
            Mana = 0,
            ArcaneEssence = 0,
            AstralShards = 0,
            LifetimeMana = 0,
            CreatedAtUtc = DateTime.UtcNow,
            LastSaveTimeUtc = DateTime.UtcNow,
            LastTickTimeUtc = DateTime.UtcNow,
            CurrentDungeonFloor = 1,
            HighestDungeonFloor = 1,
            EquippedGear = new()
            {
                { EquipmentSlot.Weapon, EquipmentGenerator.CreateStarterWand() }
            },
            Inventory = new(),
            Wizards = new List<WizardUnit>
            {
                new()
                {
                    Id = "novice",
                    Name = "Novice Apprentice",
                    Title = "Tower Scribe",
                    Description = "Recruited from the village to copy basic cantrips and channel minor mana currents.",
                    Icon = "🧙",
                    BaseCost = 15,
                    CostMultiplier = 1.15,
                    BaseMps = 1.0,
                    Count = 0
                },
                new()
                {
                    Id = "alchemist",
                    Name = "Alchemical Scholar",
                    Title = "Potion Master",
                    Description = "Distills glowing phials and transforms reagents into shimmering liquid mana.",
                    Icon = "🧪",
                    BaseCost = 100,
                    CostMultiplier = 1.15,
                    BaseMps = 8.0,
                    Count = 0
                },
                new()
                {
                    Id = "spellweaver",
                    Name = "Arcane Spellweaver",
                    Title = "Rune Shaper",
                    Description = "Weaves ethereal filaments of pure magic into enduring power grids.",
                    Icon = "🔮",
                    BaseCost = 1_100,
                    CostMultiplier = 1.15,
                    BaseMps = 48.0,
                    Count = 0
                },
                new()
                {
                    Id = "pyromancer",
                    Name = "Crimson Pyromancer",
                    Title = "Flame Warden",
                    Description = "Harnesses blazing solar firestorms to supercharge the tower's boilers.",
                    Icon = "🔥",
                    BaseCost = 12_000,
                    CostMultiplier = 1.15,
                    BaseMps = 260.0,
                    Count = 0
                },
                new()
                {
                    Id = "void_invoker",
                    Name = "Void Invoker",
                    Title = "Cosmic Channeler",
                    Description = "Taps into empty space between stars to draw limitless vacuum mana.",
                    Icon = "🌌",
                    BaseCost = 130_000,
                    CostMultiplier = 1.15,
                    BaseMps = 1_400.0,
                    Count = 0
                },
                new()
                {
                    Id = "archmage",
                    Name = "Grand Archmage Council",
                    Title = "High Magus",
                    Description = "Legendary spellcaster masters whose collective meditation distorts reality.",
                    Icon = "⚡",
                    BaseCost = 1_500_000,
                    CostMultiplier = 1.15,
                    BaseMps = 9_800.0,
                    Count = 0
                }
            },
            Upgrades = new List<Upgrade>
            {
                // Apprentice Upgrades
                new()
                {
                    Id = "upg_novice_1",
                    Name = "Illuminated Quills",
                    Description = "Novice Apprentices write spells twice as fast.",
                    Icon = "📜",
                    CostMana = 100,
                    TargetType = UpgradeTargetType.SpecificUnit,
                    TargetUnitId = "novice",
                    Multiplier = 2.0,
                    RequiredLifetimeMana = 50
                },
                new()
                {
                    Id = "upg_novice_2",
                    Name = "Arcane Coffee",
                    Description = "Keeps apprentices awake all night. Novices are 2x more effective.",
                    Icon = "☕",
                    CostMana = 500,
                    TargetType = UpgradeTargetType.SpecificUnit,
                    TargetUnitId = "novice",
                    Multiplier = 2.0,
                    RequiredLifetimeMana = 300
                },
                // Alchemist Upgrades
                new()
                {
                    Id = "upg_alch_1",
                    Name = "Refined Alembics",
                    Description = "Alchemical Scholars distill double the mana.",
                    Icon = "⚗️",
                    CostMana = 1_000,
                    TargetType = UpgradeTargetType.SpecificUnit,
                    TargetUnitId = "alchemist",
                    Multiplier = 2.0,
                    RequiredLifetimeMana = 800
                },
                // Click Upgrades
                new()
                {
                    Id = "upg_click_1",
                    Name = "Crystal Wand",
                    Description = "Arcane Orb clicks yield 2x more mana.",
                    Icon = "🪄",
                    CostMana = 250,
                    TargetType = UpgradeTargetType.ClickPower,
                    Multiplier = 2.0,
                    RequiredLifetimeMana = 100
                },
                new()
                {
                    Id = "upg_click_2",
                    Name = "Runic Focus",
                    Description = "Further channels raw focus. Clicks yield 2.5x more mana.",
                    Icon = "💎",
                    CostMana = 5_000,
                    TargetType = UpgradeTargetType.ClickPower,
                    Multiplier = 2.5,
                    RequiredLifetimeMana = 2_500
                },
                // Global Upgrades
                new()
                {
                    Id = "upg_global_1",
                    Name = "Ley Line Attunement",
                    Description = "All wizards and generators produce 25% more mana.",
                    Icon = "🌐",
                    CostMana = 25_000,
                    TargetType = UpgradeTargetType.GlobalMps,
                    Multiplier = 1.25,
                    RequiredLifetimeMana = 15_000
                },
                new()
                {
                    Id = "upg_global_2",
                    Name = "Astral Conduit",
                    Description = "Taps astral currents. All mana production is doubled.",
                    Icon = "🌟",
                    CostMana = 500_000,
                    CostEssence = 50,
                    TargetType = UpgradeTargetType.GlobalMps,
                    Multiplier = 2.0,
                    RequiredLifetimeMana = 300_000
                }
            },
            Spells = new List<Spell>
            {
                new()
                {
                    Id = "arcane_surge",
                    Name = "Arcane Surge",
                    Description = "Overcharges the magical conduit, granting 5x Click and MPS power for 20 seconds.",
                    Icon = "⚡",
                    ManaCost = 50,
                    CooldownSeconds = 90,
                    DurationSeconds = 20,
                    EffectType = SpellEffectType.ArcaneSurge,
                    PowerMultiplier = 5.0
                },
                new()
                {
                    Id = "time_warp",
                    Name = "Temporal Warp",
                    Description = "Bends the space-time continuum to immediately grant 15 minutes of passive mana.",
                    Icon = "⏳",
                    ManaCost = 500,
                    CooldownSeconds = 180,
                    DurationSeconds = 0,
                    EffectType = SpellEffectType.TimeWarp,
                    PowerMultiplier = 900.0 // 900 seconds
                },
                new()
                {
                    Id = "transmutation",
                    Name = "Transmutation",
                    Description = "Transmutes 20% of current Mana into rare Arcane Essence (minimum 1 Essence).",
                    Icon = "💠",
                    ManaCost = 200,
                    CooldownSeconds = 60,
                    DurationSeconds = 0,
                    EffectType = SpellEffectType.Transmutation,
                    PowerMultiplier = 0.20
                }
            }
        };

        return state;
    }
}
