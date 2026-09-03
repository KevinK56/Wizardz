using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public class ScrollingDungeonEngine
{
    private readonly Random _rand = new();
    private readonly SkillTreeManager _skillTree;
    private readonly IGameNotificationService _notifier;
    private GameState _state = null!;

    public const double MapWidth = 2000.0;
    public const double MapHeight = 2000.0;
    public const double ViewportWidth = 600.0;
    public const double ViewportHeight = 420.0;

    public HeroEntity Hero { get; } = new();
    public double CameraX { get; private set; } = 700.0;
    public double CameraY { get; private set; } = 790.0;

    public List<MonsterEntity> Monsters { get; } = new();
    public List<XpGemEntity> XpGems { get; } = new();
    public List<SpellProjectile> Projectiles { get; } = new();
    public List<ImpactVFX> Impacts { get; } = new();
    public List<InRunSpellState> ActiveSpells { get; } = new();
    public DungeonStairs Stairs { get; } = new();
    public TreasureChest? ActiveChest { get; set; }

    public DungeonLevelInfo CurrentLevel { get; private set; } = new();
    public int InRunLevel { get; private set; } = 1;
    public double CurrentXp { get; private set; } = 0.0;
    public double TargetXp { get; private set; } = 40.0;
    public int MonstersSlainOnFloor { get; private set; } = 0;
    public int MonstersRequiredOnFloor { get; private set; } = 15;

    public bool IsFloorCleared => Stairs.IsActive;
    public bool IsBossActive => CurrentLevel.IsBossFloor && Monsters.Any(m => m.IsBoss);
    public bool IsRunOver { get; private set; } = false;
    public bool IsPausedForDraft { get; private set; } = false;
    public List<DraftOption> CurrentDraftOptions { get; private set; } = new();

    private double _monsterSpawnTimer = 0.0;
    private double _inputVx = 0.0;
    private double _inputVy = 0.0;

    public event Action? OnStateUpdated;
    public event Action<List<DraftOption>>? OnLevelUpDraft;
    public event Action? OnRunEnded;
    public event Action<int>? OnFloorAdvanced;

    public ScrollingDungeonEngine(SkillTreeManager skillTree, IGameNotificationService notifier)
    {
        _skillTree = skillTree;
        _notifier = notifier;
    }

    public void StartNewRun(GameState state)
    {
        _state = state;
        IsRunOver = false;
        IsPausedForDraft = false;
        InRunLevel = 1;
        CurrentXp = 0.0;
        TargetXp = 40.0;
        MonstersSlainOnFloor = 0;

        CurrentLevel = new DungeonLevelInfo { FloorNumber = state.CurrentDungeonFloor };

        // Reset Hero
        Hero.WorldX = 1000.0;
        Hero.WorldY = 1000.0;
        Hero.VelocityX = 0.0;
        Hero.VelocityY = 0.0;
        Hero.InvulnerabilityTimer = 0.0;
        _skillTree.ApplyPermanentStatsToHero(state, Hero);

        // Apply equipment bonuses to Hero
        Hero.MaxHealth += state.EquippedGear.Values.Sum(g => g.AttackPower * 2.0);
        Hero.CurrentHealth = Hero.MaxHealth;
        Hero.MoveSpeed *= (1.0 + (state.EquippedGear.Values.Sum(g => g.AttackSpeedBonus) / 200.0));

        Monsters.Clear();
        XpGems.Clear();
        Projectiles.Clear();
        Impacts.Clear();
        ActiveSpells.Clear();
        ActiveChest = null;
        Stairs.IsActive = false;

        // Initialize Active Spells based on Meta Skill Tree
        InitializeStartingSpells();

        UpdateCamera();
        OnStateUpdated?.Invoke();
    }

    private void InitializeStartingSpells()
    {
        var unlocked = _skillTree.GetUnlockedSpellIds(_state);

        // Fireball
        if (unlocked.Contains("fireball"))
        {
            int metaLvl = _state.MetaSkillLevels.GetValueOrDefault("fireball", 1);
            int igniteLvl = _state.MetaSkillLevels.GetValueOrDefault("pyro_ignite", 0);
            ActiveSpells.Add(new InRunSpellState
            {
                SpellId = "fireball",
                Name = "Fireball",
                Description = "Shoots an explosive fiery blast detonating on impact.",
                Icon = "🔥",
                Element = SpellElement.Fire,
                Level = 1,
                BaseCooldown = 1.3,
                BaseDamage = 22.0 + (metaLvl * 6.0) + (igniteLvl * 4.0),
                DamagePerLevel = 12.0,
                AreaRadius = 50.0
            });
        }

        // Arcane Missiles
        if (unlocked.Contains("arcane_barrage"))
        {
            int metaLvl = _state.MetaSkillLevels.GetValueOrDefault("arcane_barrage", 1);
            ActiveSpells.Add(new InRunSpellState
            {
                SpellId = "arcane_barrage",
                Name = "Arcane Missiles",
                Description = "Rapid homing darts seeking nearest enemies.",
                Icon = "🔮",
                Element = SpellElement.Arcane,
                Level = 1,
                BaseCooldown = 0.75,
                BaseDamage = 14.0 + (metaLvl * 4.0),
                DamagePerLevel = 8.0,
                ProjectileCount = 1
            });
        }

        // Chain Lightning (if unlocked)
        if (unlocked.Contains("chain_lightning"))
        {
            int metaLvl = _state.MetaSkillLevels.GetValueOrDefault("chain_lightning", 1);
            ActiveSpells.Add(new InRunSpellState
            {
                SpellId = "chain_lightning",
                Name = "Chain Lightning",
                Description = "Discharges an electric bolt arcing between foes.",
                Icon = "⚡",
                Element = SpellElement.Lightning,
                Level = 1,
                BaseCooldown = 1.8,
                BaseDamage = 28.0 + (metaLvl * 8.0),
                DamagePerLevel = 14.0,
                ChainCount = 2
            });
        }

        // Ice Shard (if unlocked)
        if (unlocked.Contains("ice_shard"))
        {
            int metaLvl = _state.MetaSkillLevels.GetValueOrDefault("ice_shard", 1);
            ActiveSpells.Add(new InRunSpellState
            {
                SpellId = "ice_shard",
                Name = "Ice Shards",
                Description = "High-velocity piercing shards of frost.",
                Icon = "❄️",
                Element = SpellElement.Frost,
                Level = 1,
                BaseCooldown = 1.1,
                BaseDamage = 18.0 + (metaLvl * 5.0),
                DamagePerLevel = 9.0,
                ProjectileCount = 2
            });
        }
    }

    public void SetMovementInput(double moveX, double moveY)
    {
        _inputVx = moveX;
        _inputVy = moveY;

        if (Math.Abs(moveX) > 0.05 || Math.Abs(moveY) > 0.05)
        {
            Hero.FacingAngle = Math.Atan2(moveY, moveX) * (180.0 / Math.PI);
        }
    }

    public void Tick(double deltaSeconds)
    {
        if (IsRunOver || IsPausedForDraft) return;

        // 1. Move Hero
        Hero.VelocityX = _inputVx * Hero.MoveSpeed;
        Hero.VelocityY = _inputVy * Hero.MoveSpeed;

        Hero.WorldX = Math.Clamp(Hero.WorldX + Hero.VelocityX * deltaSeconds, 50.0, MapWidth - 50.0);
        Hero.WorldY = Math.Clamp(Hero.WorldY + Hero.VelocityY * deltaSeconds, 50.0, MapHeight - 50.0);

        UpdateCamera();

        if (Hero.InvulnerabilityTimer > 0)
        {
            Hero.InvulnerabilityTimer = Math.Max(0, Hero.InvulnerabilityTimer - deltaSeconds);
        }

        if (Hero.IsAttacking)
        {
            Hero.AttackAnimationTimer -= deltaSeconds;
            if (Hero.AttackAnimationTimer <= 0)
            {
                Hero.IsAttacking = false;
            }
        }

        // 2. Tick Spells Cooldowns & Fire
        TickActiveSpells(deltaSeconds);

        // 3. Update Projectiles
        TickProjectiles(deltaSeconds);

        // 4. Update Monsters Movement & Combat
        TickMonsters(deltaSeconds);

        // 5. Update Gem Magnets
        TickGems(deltaSeconds);

        // 6. Update Impact VFX
        for (int i = Impacts.Count - 1; i >= 0; i--)
        {
            var imp = Impacts[i];
            imp.Age += deltaSeconds;
            if (imp.Age >= imp.MaxAge)
            {
                Impacts.RemoveAt(i);
            }
        }

        // 7. Spawn Monsters around Hero
        _monsterSpawnTimer += deltaSeconds;
        double spawnInterval = CurrentLevel.IsBossFloor ? 2.5 : 0.8;
        if (_monsterSpawnTimer >= spawnInterval && Monsters.Count < 30 && !Stairs.IsActive)
        {
            _monsterSpawnTimer = 0;
            SpawnMonsterNearHero();
        }

        // 8. Boss Spawn Check on Floor % 10 == 0
        if (CurrentLevel.IsBossFloor && !Monsters.Any(m => m.IsBoss) && MonstersSlainOnFloor == 0)
        {
            SpawnBossNearHero();
        }

        OnStateUpdated?.Invoke();
    }

    private void UpdateCamera()
    {
        CameraX = Math.Clamp(Hero.WorldX - (ViewportWidth / 2.0), 0.0, MapWidth - ViewportWidth);
        CameraY = Math.Clamp(Hero.WorldY - (ViewportHeight / 2.0), 0.0, MapHeight - ViewportHeight);
    }

    private void TickActiveSpells(double deltaSeconds)
    {
        foreach (var spell in ActiveSpells)
        {
            spell.CooldownRemaining -= deltaSeconds;
            if (spell.CooldownRemaining <= 0 && Monsters.Any(m => !m.IsDead))
            {
                spell.CooldownRemaining = spell.CurrentCooldown;
                CastSpell(spell);
            }
        }
    }

    private void CastSpell(InRunSpellState spell)
    {
        var target = Monsters.Where(m => !m.IsDead).MinBy(m =>
        {
            double dx = m.WorldX - Hero.WorldX;
            double dy = m.WorldY - Hero.WorldY;
            return dx * dx + dy * dy;
        });

        if (target == null) return;

        Hero.IsAttacking = true;
        Hero.AttackAnimationTimer = 0.25;

        double dx = target.WorldX - Hero.WorldX;
        double dy = target.WorldY - Hero.WorldY;
        double dist = Math.Max(1.0, Math.Sqrt(dx * dx + dy * dy));

        bool isCrit = _rand.NextDouble() * 100.0 < _state.TotalCritChance;
        double dmg = spell.CurrentDamage * (_state.TotalAttackPower / 12.0);
        if (isCrit) dmg *= (_state.TotalCritDamage / 100.0);

        if (spell.SpellId == "fireball")
        {
            Projectiles.Add(new SpellProjectile
            {
                WorldX = Hero.WorldX,
                WorldY = Hero.WorldY,
                VelocityX = (dx / dist) * 440.0,
                VelocityY = (dy / dist) * 440.0,
                Damage = Math.Round(dmg),
                IsCrit = isCrit,
                Element = SpellElement.Fire,
                Icon = "🔥",
                Speed = 440.0,
                AreaRadius = spell.AreaRadius,
                Lifetime = 1.8
            });
        }
        else if (spell.SpellId == "arcane_barrage")
        {
            Projectiles.Add(new SpellProjectile
            {
                WorldX = Hero.WorldX,
                WorldY = Hero.WorldY,
                VelocityX = (dx / dist) * 520.0,
                VelocityY = (dy / dist) * 520.0,
                Damage = Math.Round(dmg * 0.75),
                IsCrit = isCrit,
                Element = SpellElement.Arcane,
                Icon = "🔮",
                Speed = 520.0,
                AreaRadius = 15.0,
                Lifetime = 1.4,
                TargetMonsterId = target.Id
            });
        }
        else if (spell.SpellId == "chain_lightning")
        {
            // Instant zap arc
            ApplyDamageToMonster(target, dmg, isCrit, SpellElement.Lightning);
            SpawnImpactVFX(target.WorldX, target.WorldY, "⚡", SpellElement.Lightning);

            // Chain to nearby monster
            var next = Monsters.Where(m => m.Id != target.Id && !m.IsDead)
                .MinBy(m => Math.Pow(m.WorldX - target.WorldX, 2) + Math.Pow(m.WorldY - target.WorldY, 2));

            if (next != null)
            {
                ApplyDamageToMonster(next, dmg * 0.8, isCrit, SpellElement.Lightning);
                SpawnImpactVFX(next.WorldX, next.WorldY, "⚡", SpellElement.Lightning);
            }
        }
        else if (spell.SpellId == "ice_shard")
        {
            Projectiles.Add(new SpellProjectile
            {
                WorldX = Hero.WorldX,
                WorldY = Hero.WorldY,
                VelocityX = (dx / dist) * 480.0,
                VelocityY = (dy / dist) * 480.0,
                Damage = Math.Round(dmg),
                IsCrit = isCrit,
                Element = SpellElement.Frost,
                Icon = "❄️",
                Speed = 480.0,
                PierceRemaining = 2,
                Lifetime = 1.5
            });
        }
    }

    private void TickProjectiles(double deltaSeconds)
    {
        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            var p = Projectiles[i];
            p.Lifetime -= deltaSeconds;
            p.WorldX += p.VelocityX * deltaSeconds;
            p.WorldY += p.VelocityY * deltaSeconds;

            if (p.Lifetime <= 0 || p.WorldX < 0 || p.WorldX > MapWidth || p.WorldY < 0 || p.WorldY > MapHeight)
            {
                Projectiles.RemoveAt(i);
                continue;
            }

            // Check collision with monsters
            foreach (var m in Monsters.Where(m => !m.IsDead))
            {
                double dist = Math.Sqrt(Math.Pow(p.WorldX - m.WorldX, 2) + Math.Pow(p.WorldY - m.WorldY, 2));
                if (dist <= (p.AreaRadius > 25 ? p.AreaRadius : 22.0))
                {
                    ApplyDamageToMonster(m, p.Damage, p.IsCrit, p.Element);
                    SpawnImpactVFX(p.WorldX, p.WorldY, p.Element == SpellElement.Fire ? "💥" : "✨", p.Element);

                    // Area splash for fireball
                    if (p.AreaRadius > 25)
                    {
                        foreach (var splash in Monsters.Where(s => s.Id != m.Id && !s.IsDead))
                        {
                            double sDist = Math.Sqrt(Math.Pow(p.WorldX - splash.WorldX, 2) + Math.Pow(p.WorldY - splash.WorldY, 2));
                            if (sDist <= p.AreaRadius)
                            {
                                ApplyDamageToMonster(splash, p.Damage * 0.6, false, p.Element);
                            }
                        }
                    }

                    if (p.PierceRemaining > 0)
                    {
                        p.PierceRemaining--;
                    }
                    else
                    {
                        Projectiles.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    private void TickMonsters(double deltaSeconds)
    {
        for (int i = Monsters.Count - 1; i >= 0; i--)
        {
            var m = Monsters[i];
            m.IsHit = false;

            if (m.IsDead)
            {
                OnMonsterDefeated(m);
                Monsters.RemoveAt(i);
                continue;
            }

            // Homing towards Hero
            double dx = Hero.WorldX - m.WorldX;
            double dy = Hero.WorldY - m.WorldY;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist > 1.0)
            {
                m.WorldX += (dx / dist) * m.Speed * deltaSeconds;
                m.WorldY += (dy / dist) * m.Speed * deltaSeconds;
            }

            // Contact damage to Hero
            if (dist <= 26.0 && !Hero.IsInvulnerable)
            {
                Hero.CurrentHealth -= m.Damage;
                Hero.InvulnerabilityTimer = 0.6;
                SpawnImpactVFX(Hero.WorldX, Hero.WorldY, "🩸", SpellElement.Arcane);

                if (Hero.CurrentHealth <= 0)
                {
                    Hero.CurrentHealth = 0;
                    IsRunOver = true;
                    OnRunEnded?.Invoke();
                    break;
                }
            }
        }
    }

    private void TickGems(double deltaSeconds)
    {
        for (int i = XpGems.Count - 1; i >= 0; i--)
        {
            var g = XpGems[i];
            double dx = Hero.WorldX - g.WorldX;
            double dy = Hero.WorldY - g.WorldY;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist <= Hero.MagnetRadius || g.IsMagnetized)
            {
                g.IsMagnetized = true;
                double pullSpeed = 320.0;
                g.WorldX += (dx / dist) * pullSpeed * deltaSeconds;
                g.WorldY += (dy / dist) * pullSpeed * deltaSeconds;

                if (dist <= 22.0)
                {
                    // Collect Gem
                    CollectGem(g);
                    XpGems.RemoveAt(i);
                }
            }
        }
    }

    private void CollectGem(XpGemEntity gem)
    {
        double manaYield = gem.Value * _state.TotalManaFind;
        _state.Mana += manaYield;
        _state.LifetimeMana += manaYield;

        CurrentXp += gem.Value;
        if (CurrentXp >= TargetXp)
        {
            CurrentXp -= TargetXp;
            InRunLevel++;
            TargetXp = Math.Round(TargetXp * 1.35);
            TriggerLevelUpDraft();
        }
    }

    private void TriggerLevelUpDraft()
    {
        IsPausedForDraft = true;
        CurrentDraftOptions = GenerateDraftOptions();
        OnLevelUpDraft?.Invoke(CurrentDraftOptions);
    }

    private List<DraftOption> GenerateDraftOptions()
    {
        var options = new List<DraftOption>();
        var unlocked = _skillTree.GetUnlockedSpellIds(_state);

        // 1. Upgrade existing in-run spells
        foreach (var spell in ActiveSpells.Where(s => s.Level < s.MaxLevel))
        {
            options.Add(new DraftOption
            {
                SpellId = spell.SpellId,
                Title = $"Upgrade {spell.Name}",
                Description = spell.GetUpgradeSummary(),
                Icon = spell.Icon,
                RarityLabel = "UPGRADE",
                CurrentLevel = spell.Level,
                TargetLevel = spell.Level + 1,
                IsNewUnlock = false,
                Element = spell.Element
            });
        }

        // 2. Offer new unlocked spells not yet drafted in this run
        var currentSpellIds = ActiveSpells.Select(s => s.SpellId).ToHashSet();
        foreach (var spellId in unlocked.Where(id => !currentSpellIds.Contains(id)))
        {
            var node = _skillTree.AllNodes.FirstOrDefault(n => n.AssociatedSpellId == spellId);
            if (node != null)
            {
                options.Add(new DraftOption
                {
                    SpellId = spellId,
                    Title = $"Learn {node.Name}",
                    Description = node.Description,
                    Icon = node.Icon,
                    RarityLabel = "NEW SPELL",
                    CurrentLevel = 0,
                    TargetLevel = 1,
                    IsNewUnlock = true,
                    Element = node.Branch switch
                    {
                        SkillBranch.Pyromancy => SpellElement.Fire,
                        SkillBranch.Electromancy => SpellElement.Lightning,
                        SkillBranch.Cryomancy => SpellElement.Frost,
                        _ => SpellElement.Arcane
                    }
                });
            }
        }

        // 3. Fallback stat boosts if fewer than 3 options
        if (options.Count < 3)
        {
            options.Add(new DraftOption
            {
                SpellId = "heal_vitality",
                Title = "Vital Surge",
                Description = "Restores 40% of maximum health immediately.",
                Icon = "💖",
                RarityLabel = "HEAL",
                IsNewUnlock = false,
                Element = SpellElement.Arcane
            });
        }

        if (options.Count < 3)
        {
            options.Add(new DraftOption
            {
                SpellId = "move_speed",
                Title = "Fleetfoot Elixir",
                Description = "+12% Movement speed for the remainder of this run.",
                Icon = "👟",
                RarityLabel = "STAT",
                IsNewUnlock = false,
                Element = SpellElement.Arcane
            });
        }

        return options.OrderBy(_ => _rand.Next()).Take(3).ToList();
    }

    public void SelectDraftOption(DraftOption option)
    {
        if (option.SpellId == "heal_vitality")
        {
            Hero.CurrentHealth = Math.Min(Hero.MaxHealth, Hero.CurrentHealth + Hero.MaxHealth * 0.4);
        }
        else if (option.SpellId == "move_speed")
        {
            Hero.MoveSpeed *= 1.12;
        }
        else if (option.IsNewUnlock)
        {
            // Add new in-run spell
            var newSpell = CreateSpellState(option.SpellId);
            if (newSpell != null) ActiveSpells.Add(newSpell);
        }
        else
        {
            var spell = ActiveSpells.FirstOrDefault(s => s.SpellId == option.SpellId);
            if (spell != null && spell.Level < spell.MaxLevel)
            {
                spell.Level++;
            }
        }

        IsPausedForDraft = false;
        CurrentDraftOptions.Clear();
        OnStateUpdated?.Invoke();
    }

    private InRunSpellState? CreateSpellState(string spellId)
    {
        return spellId switch
        {
            "fireball" => new InRunSpellState { SpellId = "fireball", Name = "Fireball", Icon = "🔥", Element = SpellElement.Fire, BaseCooldown = 1.3, BaseDamage = 24.0, DamagePerLevel = 12.0, AreaRadius = 50.0 },
            "chain_lightning" => new InRunSpellState { SpellId = "chain_lightning", Name = "Chain Lightning", Icon = "⚡", Element = SpellElement.Lightning, BaseCooldown = 1.8, BaseDamage = 30.0, DamagePerLevel = 14.0, ChainCount = 2 },
            "ice_shard" => new InRunSpellState { SpellId = "ice_shard", Name = "Ice Shards", Icon = "❄️", Element = SpellElement.Frost, BaseCooldown = 1.1, BaseDamage = 20.0, DamagePerLevel = 10.0, ProjectileCount = 2 },
            "arcane_barrage" => new InRunSpellState { SpellId = "arcane_barrage", Name = "Arcane Missiles", Icon = "🔮", Element = SpellElement.Arcane, BaseCooldown = 0.75, BaseDamage = 16.0, DamagePerLevel = 8.0 },
            "frost_nova" => new InRunSpellState { SpellId = "frost_nova", Name = "Frost Nova", Icon = "🧊", Element = SpellElement.Frost, BaseCooldown = 2.8, BaseDamage = 35.0, DamagePerLevel = 18.0, AreaRadius = 90.0 },
            "meteor" => new InRunSpellState { SpellId = "meteor", Name = "Meteor Shower", Icon = "☄️", Element = SpellElement.Fire, BaseCooldown = 3.5, BaseDamage = 65.0, DamagePerLevel = 30.0, AreaRadius = 80.0 },
            _ => null
        };
    }

    private void ApplyDamageToMonster(MonsterEntity monster, double damage, bool isCrit, SpellElement element)
    {
        monster.CurrentHealth = Math.Max(0, monster.CurrentHealth - damage);
        monster.IsHit = true;
    }

    private void SpawnImpactVFX(double x, double y, string icon, SpellElement element)
    {
        Impacts.Add(new ImpactVFX
        {
            WorldX = x,
            WorldY = y,
            Icon = icon,
            Element = element
        });
    }

    private void OnMonsterDefeated(MonsterEntity monster)
    {
        MonstersSlainOnFloor++;

        // Drop Gem
        XpGems.Add(new XpGemEntity
        {
            WorldX = monster.WorldX,
            WorldY = monster.WorldY,
            Value = monster.XpReward
        });

        // Boss drops equipment chest
        if (monster.IsBoss)
        {
            var bossGear = EquipmentGenerator.GenerateLoot(CurrentLevel.FloorNumber, ItemRarity.Rare);
            _state.Inventory.Add(bossGear);
            SpawnChestAt(monster.WorldX, monster.WorldY);
            OpenStairs(monster.WorldX, monster.WorldY);
        }
        else if (MonstersSlainOnFloor >= MonstersRequiredOnFloor && !Stairs.IsActive && !CurrentLevel.IsBossFloor)
        {
            OpenStairs(monster.WorldX, monster.WorldY);
        }
    }

    private void SpawnChestAt(double x, double y)
    {
        ActiveChest = new TreasureChest
        {
            Id = Guid.NewGuid().ToString(),
            Tier = ChestTier.Gold,
            Name = "Gilded Boss Chest",
            X = x,
            Y = y,
            ManaReward = Math.Round((150.0 + (CurrentLevel.FloorNumber * 50.0)) * _state.TotalManaFind),
            EssenceReward = 5
        };
    }

    private void OpenStairs(double x, double y)
    {
        Stairs.WorldX = x;
        Stairs.WorldY = y;
        Stairs.IsActive = true;
        _ = _notifier.BroadcastStateAsync($"Floor {CurrentLevel.FloorNumber} Cleared! Stairs Unlocked.");
    }

    public void DescendToNextFloor()
    {
        if (!Stairs.IsActive) return;

        _state.CurrentDungeonFloor++;
        if (_state.CurrentDungeonFloor > _state.HighestDungeonFloor)
        {
            _state.HighestDungeonFloor = _state.CurrentDungeonFloor;
        }

        CurrentLevel = new DungeonLevelInfo { FloorNumber = _state.CurrentDungeonFloor };
        MonstersSlainOnFloor = 0;
        Stairs.IsActive = false;
        Monsters.Clear();
        Projectiles.Clear();

        OnFloorAdvanced?.Invoke(_state.CurrentDungeonFloor);
    }

    private void SpawnMonsterNearHero()
    {
        double angle = _rand.NextDouble() * Math.PI * 2.0;
        double spawnDist = 380.0; // just outside viewport

        double x = Math.Clamp(Hero.WorldX + Math.Cos(angle) * spawnDist, 60.0, MapWidth - 60.0);
        double y = Math.Clamp(Hero.WorldY + Math.Sin(angle) * spawnDist, 60.0, MapHeight - 60.0);

        int floor = CurrentLevel.FloorNumber;
        double hp = (25.0 + (floor * 12.0)) * Math.Pow(1.05, floor / 2.0);

        var (name, icon) = GetMonsterNameAndIcon(CurrentLevel.Biome);

        Monsters.Add(new MonsterEntity
        {
            Name = name,
            Icon = icon,
            WorldX = x,
            WorldY = y,
            MaxHealth = Math.Round(hp),
            CurrentHealth = Math.Round(hp),
            Speed = 65.0 + _rand.NextDouble() * 20.0,
            Damage = 10.0 + (floor * 2.0),
            XpReward = 10.0 + (floor * 2.0),
            ManaReward = 15.0 + (floor * 5.0)
        });
    }

    private void SpawnBossNearHero()
    {
        double x = Math.Clamp(Hero.WorldX, 200.0, MapWidth - 200.0);
        double y = Math.Clamp(Hero.WorldY - 250.0, 200.0, MapHeight - 200.0);

        int floor = CurrentLevel.FloorNumber;
        double hp = (350.0 + (floor * 120.0)) * Math.Pow(1.08, floor / 2.0);

        Monsters.Add(new MonsterEntity
        {
            Name = CurrentLevel.Biome switch
            {
                DungeonBiome.MossyCatacombs => "💀 Ancient Bone Sovereign",
                DungeonBiome.SunkenCrypt => "👑 Arch-Lich Malakar",
                DungeonBiome.MagmaCaldera => "🐉 Pyroclast Drake",
                _ => "👁️ Cosmic Titan"
            },
            Icon = CurrentLevel.Biome switch
            {
                DungeonBiome.MossyCatacombs => "💀",
                DungeonBiome.SunkenCrypt => "👑",
                DungeonBiome.MagmaCaldera => "🐉",
                _ => "👁️"
            },
            WorldX = x,
            WorldY = y,
            MaxHealth = Math.Round(hp),
            CurrentHealth = Math.Round(hp),
            Speed = 50.0,
            Damage = 25.0 + (floor * 4.0),
            XpReward = 120.0,
            ManaReward = 250.0,
            IsBoss = true
        });
    }

    private (string Name, string Icon) GetMonsterNameAndIcon(DungeonBiome biome)
    {
        string[][] list = {
            new[] { "Moss Slime:🟢", "Catacomb Skeleton:🦴", "Cave Bat:🦇", "Goblin Thief:👺" },
            new[] { "Shadow Wraith:👻", "Crypt Ghoul:🧟", "Necromancer:🧙‍♀️", "Cursed Skull:💀" },
            new[] { "Lava Elemental:🔥", "Fire Imp:😈", "Magma Hound:🐕‍🦺", "Molten Worm:🐛" },
            new[] { "Void Spider:🕷️", "Astral Specter:🌌", "Cosmic Orb:🔮", "Star Devourer:👾" }
        };

        var set = list[(int)biome];
        string pick = set[_rand.Next(0, set.Length)];
        var parts = pick.Split(':');
        return (parts[0], parts[1]);
    }
}
