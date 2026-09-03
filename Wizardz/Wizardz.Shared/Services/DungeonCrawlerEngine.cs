using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public class DungeonCrawlerEngine
{
    private readonly Random _rand = new();
    private GameState _state = null!;
    private readonly IGameNotificationService _notifier;

    public double HeroX { get; set; } = 50.0; // Percentage 0 - 100
    public double HeroY { get; set; } = 50.0;
    public double HeroFacingAngle { get; set; } = 0.0; // Degrees

    public List<DungeonMonster> ActiveMonsters { get; } = new();
    public List<MagicProjectile> ActiveProjectiles { get; } = new();
    public List<CombatFloatingNumber> FloatingTexts { get; } = new();
    public TreasureChest? ActiveChest { get; set; }

    public DungeonLevelInfo CurrentLevel { get; private set; } = new();
    public int MonstersKilledInRoom { get; private set; } = 0;
    public int MonstersRequiredForRoom { get; private set; } = 5;
    public bool IsRoomCleared => MonstersKilledInRoom >= MonstersRequiredForRoom && ActiveMonsters.Count == 0;
    public bool IsBossActive => CurrentLevel.IsBossFloor && ActiveMonsters.Any(m => m.IsBoss);

    private double _attackCooldownTimer = 0.0;

    public event Action? OnDungeonUpdated;
    public event Action<TreasureChest>? OnChestOpened;
    public event Action<int>? OnFloorChanged;

    public DungeonCrawlerEngine(IGameNotificationService notifier)
    {
        _notifier = notifier;
    }

    public void Initialize(GameState state)
    {
        _state = state;
        CurrentLevel = new DungeonLevelInfo { FloorNumber = state.CurrentDungeonFloor };
        ActiveChest = state.ActiveChest;
        StartNewRoom();
    }

    public void StartNewRoom()
    {
        ActiveMonsters.Clear();
        ActiveProjectiles.Clear();
        MonstersKilledInRoom = 0;

        if (CurrentLevel.IsBossFloor)
        {
            MonstersRequiredForRoom = 1;
            SpawnBoss();
        }
        else
        {
            MonstersRequiredForRoom = 4 + (_rand.Next(0, 3)); // 4-6 monsters
            SpawnMonsterWave();
        }

        // Random chance (25%) to spawn a treasure chest in the room if none exists
        if (ActiveChest == null && _rand.Next(0, 100) < 25)
        {
            SpawnChest();
        }

        OnDungeonUpdated?.Invoke();
    }

    private void SpawnMonsterWave()
    {
        int toSpawn = Math.Min(3, MonstersRequiredForRoom - MonstersKilledInRoom - ActiveMonsters.Count);
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        double angle = _rand.NextDouble() * Math.PI * 2.0;
        double spawnDist = 42.0; // near room edge

        double x = Math.Clamp(HeroX + Math.Cos(angle) * spawnDist, 10, 90);
        double y = Math.Clamp(HeroY + Math.Sin(angle) * spawnDist, 10, 90);

        int floor = CurrentLevel.FloorNumber;
        double baseHp = (30.0 + (floor * 18.0)) * Math.Pow(1.06, floor / 2.0);

        var (name, icon) = GetMonsterThemeName(CurrentLevel.Biome);

        var monster = new DungeonMonster
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Icon = icon,
            MaxHealth = Math.Round(baseHp),
            CurrentHealth = Math.Round(baseHp),
            X = x,
            Y = y,
            Speed = 8.0 + _rand.NextDouble() * 4.0,
            ManaReward = Math.Round((15.0 + (floor * 8.0)) * _state.TotalManaFind),
            EssenceReward = floor >= 10 && _rand.Next(0, 100) < 15 ? 1 : 0,
            IsBoss = false
        };

        ActiveMonsters.Add(monster);
    }

    private void SpawnBoss()
    {
        int floor = CurrentLevel.FloorNumber;
        double bossHp = (300.0 + (floor * 120.0)) * Math.Pow(1.08, floor / 2.0);

        string bossName = CurrentLevel.Biome switch
        {
            DungeonBiome.MossyCatacombs => "Ancient Bone Golem Lord",
            DungeonBiome.SunkenCrypt => "Arch-Lich Malakar",
            DungeonBiome.MagmaCaldera => "Magma Drake Sovereign",
            _ => "Eldritch Void Behemoth"
        };

        string bossIcon = CurrentLevel.Biome switch
        {
            DungeonBiome.MossyCatacombs => "💀",
            DungeonBiome.SunkenCrypt => "👑",
            DungeonBiome.MagmaCaldera => "🐉",
            _ => "👁️"
        };

        var boss = new DungeonMonster
        {
            Id = Guid.NewGuid().ToString(),
            Name = bossName,
            Icon = bossIcon,
            MaxHealth = Math.Round(bossHp),
            CurrentHealth = Math.Round(bossHp),
            X = 50.0,
            Y = 18.0, // Top chamber
            Speed = 6.0,
            ManaReward = Math.Round((150.0 + (floor * 60.0)) * _state.TotalManaFind),
            EssenceReward = 5 + (floor / 5),
            IsBoss = true
        };

        ActiveMonsters.Add(boss);
    }

    private (string Name, string Icon) GetMonsterThemeName(DungeonBiome biome)
    {
        string[][] monsterSets = {
            new[] { "Moss Slime:🟢", "Catacomb Skeleton:🦴", "Cave Bat:🦇", "Goblin Thief:👺" },
            new[] { "Shadow Wraith:👻", "Crypt Ghoul:🧟", "Necromancer Adept:🧙‍♀️", "Cursed Skull:💀" },
            new[] { "Lava Elemental:🔥", "Fire Imp:😈", "Magma Hound:🐕‍🦺", "Molten Worm:🐛" },
            new[] { "Void Spider:🕷️", "Astral Specter:🌌", "Cosmic Orb:🔮", "Star Devourer:👾" }
        };

        var set = monsterSets[(int)biome];
        string pick = set[_rand.Next(0, set.Length)];
        var parts = pick.Split(':');
        return (parts[0], parts[1]);
    }

    public void SpawnChest()
    {
        int floor = CurrentLevel.FloorNumber;
        int roll = _rand.Next(0, 100);

        ChestTier tier;
        string name;
        if (roll < 10)
        {
            tier = ChestTier.Arcane;
            name = "Arcane Mystery Chest";
        }
        else if (roll < 35)
        {
            tier = ChestTier.Gold;
            name = "Gilded Treasure Chest";
        }
        else
        {
            tier = ChestTier.Wood;
            name = "Dungeon Wooden Chest";
        }

        ActiveChest = new TreasureChest
        {
            Id = Guid.NewGuid().ToString(),
            Tier = tier,
            Name = name,
            X = _rand.Next(25, 75),
            Y = _rand.Next(25, 75),
            ManaReward = Math.Round((50.0 + (floor * 30.0)) * ((int)tier + 1) * _state.TotalManaFind),
            EssenceReward = tier >= ChestTier.Gold ? (int)tier : 0,
            DroppedItem = tier >= ChestTier.Gold || _rand.Next(0, 100) < 40 
                ? EquipmentGenerator.GenerateLoot(floor, tier == ChestTier.Arcane ? ItemRarity.Rare : null) 
                : null
        };

        _state.ActiveChest = ActiveChest;
    }

    public void OpenChest()
    {
        if (ActiveChest == null || ActiveChest.IsOpened) return;

        ActiveChest.IsOpened = true;
        _state.Mana += ActiveChest.ManaReward;
        _state.LifetimeMana += ActiveChest.ManaReward;
        _state.ArcaneEssence += ActiveChest.EssenceReward;

        if (ActiveChest.DroppedItem != null)
        {
            _state.Inventory.Add(ActiveChest.DroppedItem);
        }

        var openedChest = ActiveChest;
        ActiveChest = null;
        _state.ActiveChest = null;

        OnChestOpened?.Invoke(openedChest);
        OnDungeonUpdated?.Invoke();
        _ = _notifier.NotifyAffordabilityChangedAsync();
    }

    public void Tick(double deltaSeconds)
    {
        // 1. Update Projectiles
        for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
        {
            var p = ActiveProjectiles[i];
            p.Progress += (p.Speed * deltaSeconds) / 100.0;
            p.CurrentX = p.StartX + (p.TargetX - p.StartX) * Math.Min(1.0, p.Progress);
            p.CurrentY = p.StartY + (p.TargetY - p.StartY) * Math.Min(1.0, p.Progress);

            if (p.Progress >= 1.0)
            {
                // Impact target
                var monster = ActiveMonsters.FirstOrDefault(m => m.Id == p.TargetMonsterId);
                if (monster != null && !monster.IsDead)
                {
                    ApplyDamageToMonster(monster, p.Damage, p.IsCrit);
                }
                ActiveProjectiles.RemoveAt(i);
            }
        }

        // 2. Update Monsters Movement & Cleanup
        for (int i = ActiveMonsters.Count - 1; i >= 0; i--)
        {
            var m = ActiveMonsters[i];
            m.IsHit = false;

            if (m.IsDead)
            {
                OnMonsterDefeated(m);
                ActiveMonsters.RemoveAt(i);
                continue;
            }

            // Move slightly towards Hero
            double dx = HeroX - m.X;
            double dy = HeroY - m.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist > 14.0) // Keep small distance from wizard
            {
                m.X += (dx / dist) * m.Speed * deltaSeconds;
                m.Y += (dy / dist) * m.Speed * deltaSeconds;
            }
        }

        // 3. Auto Spawn Next Wave if room still has enemies remaining
        if (!CurrentLevel.IsBossFloor && ActiveMonsters.Count < 2 && (MonstersKilledInRoom + ActiveMonsters.Count) < MonstersRequiredForRoom)
        {
            SpawnMonsterWave();
        }

        // 4. Hero Auto-Attack
        _attackCooldownTimer -= deltaSeconds;
        double attackInterval = 1.0 / Math.Max(0.5, _state.TotalAttackSpeed);

        if (_attackCooldownTimer <= 0)
        {
            _attackCooldownTimer = attackInterval;
            TryAutoAttack();
        }

        // 5. Update Floating Texts
        for (int i = FloatingTexts.Count - 1; i >= 0; i--)
        {
            var ft = FloatingTexts[i];
            ft.Age += deltaSeconds;
            ft.Y -= deltaSeconds * 20.0;
            if (ft.Age >= 0.8)
            {
                FloatingTexts.RemoveAt(i);
            }
        }

        OnDungeonUpdated?.Invoke();
    }

    private void TryAutoAttack()
    {
        var target = ActiveMonsters.Where(m => !m.IsDead).MinBy(m =>
        {
            double dx = m.X - HeroX;
            double dy = m.Y - HeroY;
            return dx * dx + dy * dy;
        });

        if (target == null) return;

        // Calculate Angle
        double dx = target.X - HeroX;
        double dy = target.Y - HeroY;
        HeroFacingAngle = Math.Atan2(dy, dx) * (180.0 / Math.PI);

        // Roll Crit
        bool isCrit = _rand.NextDouble() * 100.0 < _state.TotalCritChance;
        double damage = _state.TotalAttackPower;
        if (isCrit)
        {
            damage *= (_state.TotalCritDamage / 100.0);
        }

        // Launch Projectile
        var projectile = new MagicProjectile
        {
            StartX = HeroX,
            StartY = HeroY,
            CurrentX = HeroX,
            CurrentY = HeroY,
            TargetX = target.X,
            TargetY = target.Y,
            TargetMonsterId = target.Id,
            Damage = Math.Round(damage),
            IsCrit = isCrit,
            Icon = isCrit ? "⚡" : "✨"
        };

        ActiveProjectiles.Add(projectile);
    }

    public void ClickAttackMonster(string monsterId)
    {
        var target = ActiveMonsters.FirstOrDefault(m => m.Id == monsterId);
        if (target == null || target.IsDead) return;

        // Instant click strike
        bool isCrit = _rand.NextDouble() * 100.0 < (_state.TotalCritChance + 10.0);
        double damage = Math.Max(1.0, _state.ClickManaGain * 1.5 + _state.TotalAttackPower * 0.8);
        if (isCrit) damage *= 2.0;

        ApplyDamageToMonster(target, Math.Round(damage), isCrit);
    }

    private void ApplyDamageToMonster(DungeonMonster monster, double damage, bool isCrit)
    {
        monster.CurrentHealth = Math.Max(0, monster.CurrentHealth - damage);
        monster.IsHit = true;

        FloatingTexts.Add(new CombatFloatingNumber
        {
            X = monster.X + (_rand.NextDouble() * 8.0 - 4.0),
            Y = monster.Y - 4.0,
            Text = $"-{GameEngine.FormatNumber(damage)}",
            IsCrit = isCrit
        });
    }

    private void OnMonsterDefeated(DungeonMonster monster)
    {
        MonstersKilledInRoom++;
        _state.Mana += monster.ManaReward;
        _state.LifetimeMana += monster.ManaReward;
        _state.ArcaneEssence += monster.EssenceReward;

        // Boss drops guaranteed rare+ equipment!
        if (monster.IsBoss)
        {
            var bossGear = EquipmentGenerator.GenerateLoot(CurrentLevel.FloorNumber, ItemRarity.Rare);
            _state.Inventory.Add(bossGear);
            SpawnChest(); // Bonus chest
        }

        _ = _notifier.NotifyAffordabilityChangedAsync();
    }

    public void DescendToNextFloor()
    {
        if (!IsRoomCleared) return;

        _state.CurrentDungeonFloor++;
        if (_state.CurrentDungeonFloor > _state.HighestDungeonFloor)
        {
            _state.HighestDungeonFloor = _state.CurrentDungeonFloor;
        }

        CurrentLevel = new DungeonLevelInfo { FloorNumber = _state.CurrentDungeonFloor };
        StartNewRoom();

        OnFloorChanged?.Invoke(_state.CurrentDungeonFloor);
    }
}

public class CombatFloatingNumber
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCrit { get; set; }
    public double Age { get; set; } = 0.0;
}
