using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public class SkillTreeManager
{
    public List<SkillNode> AllNodes { get; } = new();
    public List<ConstellationLine> AllLines { get; } = new();

    public SkillTreeManager()
    {
        InitializeTree();
        BuildConstellationLines();
    }

    private void InitializeTree()
    {
        // 1. Pyromancy Branch (The Phoenix - Top-Left)
        AllNodes.Add(new SkillNode
        {
            Id = "fireball",
            Name = "Heart of Fireball",
            Description = "Hurls an explosive sphere of fire that detonates upon impact.",
            Icon = "🔥",
            Branch = SkillBranch.Pyromancy,
            BaseCost = 50,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "fireball",
            ConstellationX = 240,
            ConstellationY = 260,
            IsMajorStar = true
        });

        AllNodes.Add(new SkillNode
        {
            Id = "pyro_ignite",
            Name = "Ignite Spark",
            Description = "+20% Fire spell damage and burning trails.",
            Icon = "♨️",
            Branch = SkillBranch.Pyromancy,
            BaseCost = 120,
            MaxLevel = 5,
            PrerequisiteNodeId = "fireball",
            StatBonusPerLevel = 20.0,
            ConstellationX = 160,
            ConstellationY = 180,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "pyro_flame_wave",
            Name = "Flame Nova",
            Description = "+15% Fire area of effect explosion radius.",
            Icon = "🎇",
            Branch = SkillBranch.Pyromancy,
            BaseCost = 180,
            MaxLevel = 5,
            PrerequisiteNodeId = "fireball",
            StatBonusPerLevel = 15.0,
            ConstellationX = 310,
            ConstellationY = 170,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "pyro_meteor",
            Name = "Meteor Shower",
            Description = "Calls blazing celestial meteors from the sky crushing enemy swarms.",
            Icon = "☄️",
            Branch = SkillBranch.Pyromancy,
            BaseCost = 350,
            MaxLevel = 5,
            PrerequisiteNodeId = "pyro_ignite",
            IsSpellUnlock = true,
            AssociatedSpellId = "meteor",
            ConstellationX = 220,
            ConstellationY = 90,
            IsMajorStar = true
        });

        // 2. Electromancy Branch (The Thunder Drake - Top-Right)
        AllNodes.Add(new SkillNode
        {
            Id = "chain_lightning",
            Name = "Arc Lightning",
            Description = "Discharges electric arcs that jump between multiple nearby monsters.",
            Icon = "⚡",
            Branch = SkillBranch.Electromancy,
            BaseCost = 75,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "chain_lightning",
            ConstellationX = 760,
            ConstellationY = 260,
            IsMajorStar = true
        });

        AllNodes.Add(new SkillNode
        {
            Id = "electro_overcharge",
            Name = "Overcharge Coils",
            Description = "+10% Faster cooldown recovery for all spells.",
            Icon = "🔋",
            Branch = SkillBranch.Electromancy,
            BaseCost = 150,
            MaxLevel = 5,
            PrerequisiteNodeId = "chain_lightning",
            StatBonusPerLevel = 10.0,
            ConstellationX = 840,
            ConstellationY = 180,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "electro_shock",
            Name = "Static Discharge",
            Description = "+15% Critical strike multiplier on lightning arcs.",
            Icon = "⚡",
            Branch = SkillBranch.Electromancy,
            BaseCost = 220,
            MaxLevel = 5,
            PrerequisiteNodeId = "chain_lightning",
            StatBonusPerLevel = 15.0,
            ConstellationX = 680,
            ConstellationY = 170,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "electro_storm",
            Name = "Tempest Storm",
            Description = "Summons an active roaming thunderstorm that automatically strikes foes.",
            Icon = "🌩️",
            Branch = SkillBranch.Electromancy,
            BaseCost = 400,
            MaxLevel = 5,
            PrerequisiteNodeId = "electro_overcharge",
            IsSpellUnlock = true,
            AssociatedSpellId = "storm",
            ConstellationX = 770,
            ConstellationY = 90,
            IsMajorStar = true
        });

        // 3. Cryomancy Branch (The Frost Serpent - Bottom-Left)
        AllNodes.Add(new SkillNode
        {
            Id = "ice_shard",
            Name = "Ice Shards",
            Description = "Fires crystalline piercing shards of frost in a forward spread.",
            Icon = "❄️",
            Branch = SkillBranch.Cryomancy,
            BaseCost = 60,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "ice_shard",
            ConstellationX = 240,
            ConstellationY = 560,
            IsMajorStar = true
        });

        AllNodes.Add(new SkillNode
        {
            Id = "cryo_frost_nova",
            Name = "Frost Nova",
            Description = "Erupts in a radial wave of freezing ice pushing away all surrounding monsters.",
            Icon = "🧊",
            Branch = SkillBranch.Cryomancy,
            BaseCost = 140,
            MaxLevel = 5,
            PrerequisiteNodeId = "ice_shard",
            IsSpellUnlock = true,
            AssociatedSpellId = "frost_nova",
            ConstellationX = 160,
            ConstellationY = 640,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "cryo_permafrost",
            Name = "Permafrost Chill",
            Description = "+20% Damage against chilled and slowed monsters.",
            Icon = "💎",
            Branch = SkillBranch.Cryomancy,
            BaseCost = 240,
            MaxLevel = 5,
            PrerequisiteNodeId = "ice_shard",
            StatBonusPerLevel = 20.0,
            ConstellationX = 320,
            ConstellationY = 650,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "cryo_blizzard",
            Name = "Howling Blizzard",
            Description = "Surrounds the wizard in a permanent sub-zero vortex that chills and damages.",
            Icon = "🌨️",
            Branch = SkillBranch.Cryomancy,
            BaseCost = 450,
            MaxLevel = 5,
            PrerequisiteNodeId = "cryo_frost_nova",
            IsSpellUnlock = true,
            AssociatedSpellId = "blizzard",
            ConstellationX = 230,
            ConstellationY = 740,
            IsMajorStar = true
        });

        // 4. Arcane Mastery Branch (The Cosmic Eye - Center)
        AllNodes.Add(new SkillNode
        {
            Id = "arcane_barrage",
            Name = "Arcane Core",
            Description = "Rapidly fires homing arcane darts seeking out nearest enemies.",
            Icon = "🔮",
            Branch = SkillBranch.ArcaneMastery,
            BaseCost = 50,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "arcane_barrage",
            ConstellationX = 500,
            ConstellationY = 400,
            IsMajorStar = true
        });

        AllNodes.Add(new SkillNode
        {
            Id = "arcane_echo",
            Name = "Cosmic Echo",
            Description = "+8% chance per rank to cast any spell twice simultaneously.",
            Icon = "🌀",
            Branch = SkillBranch.ArcaneMastery,
            BaseCost = 200,
            MaxLevel = 5,
            PrerequisiteNodeId = "arcane_barrage",
            StatBonusPerLevel = 8.0,
            ConstellationX = 500,
            ConstellationY = 290,
            IsMajorStar = false
        });

        // 5. Vitality Branch (The Colossus - Bottom-Right)
        AllNodes.Add(new SkillNode
        {
            Id = "vitality_hp",
            Name = "Heart of Iron",
            Description = "+25 Base Max Health for all future dungeon runs.",
            Icon = "❤️",
            Branch = SkillBranch.Vitality,
            BaseCost = 40,
            MaxLevel = 10,
            StatBonusPerLevel = 25.0,
            ConstellationX = 760,
            ConstellationY = 550,
            IsMajorStar = true
        });

        AllNodes.Add(new SkillNode
        {
            Id = "vitality_speed",
            Name = "Zephyr Stride",
            Description = "+8% Movement speed to outmaneuver dungeon swarms.",
            Icon = "👟",
            Branch = SkillBranch.Vitality,
            BaseCost = 50,
            MaxLevel = 10,
            StatBonusPerLevel = 8.0,
            ConstellationX = 850,
            ConstellationY = 630,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "vitality_magnet",
            Name = "Astral Attractor",
            Description = "+25% Pickup radius to draw XP and Mana gems effortlessly.",
            Icon = "🧲",
            Branch = SkillBranch.Vitality,
            BaseCost = 45,
            MaxLevel = 10,
            StatBonusPerLevel = 25.0,
            ConstellationX = 670,
            ConstellationY = 640,
            IsMajorStar = false
        });

        AllNodes.Add(new SkillNode
        {
            Id = "vitality_mana_find",
            Name = "Alchemical Greed",
            Description = "+15% Bonus Mana and XP gained from all fallen monsters and chests.",
            Icon = "💰",
            Branch = SkillBranch.Vitality,
            BaseCost = 60,
            MaxLevel = 10,
            StatBonusPerLevel = 15.0,
            ConstellationX = 760,
            ConstellationY = 730,
            IsMajorStar = true
        });
    }

    private void BuildConstellationLines()
    {
        AllLines.Clear();

        // Pyromancy lines
        AllLines.Add(new ConstellationLine { FromNodeId = "fireball", ToNodeId = "pyro_ignite", Branch = SkillBranch.Pyromancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "fireball", ToNodeId = "pyro_flame_wave", Branch = SkillBranch.Pyromancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "pyro_ignite", ToNodeId = "pyro_meteor", Branch = SkillBranch.Pyromancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "pyro_flame_wave", ToNodeId = "pyro_meteor", Branch = SkillBranch.Pyromancy });

        // Electromancy lines
        AllLines.Add(new ConstellationLine { FromNodeId = "chain_lightning", ToNodeId = "electro_overcharge", Branch = SkillBranch.Electromancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "chain_lightning", ToNodeId = "electro_shock", Branch = SkillBranch.Electromancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "electro_overcharge", ToNodeId = "electro_storm", Branch = SkillBranch.Electromancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "electro_shock", ToNodeId = "electro_storm", Branch = SkillBranch.Electromancy });

        // Cryomancy lines
        AllLines.Add(new ConstellationLine { FromNodeId = "ice_shard", ToNodeId = "cryo_frost_nova", Branch = SkillBranch.Cryomancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "ice_shard", ToNodeId = "cryo_permafrost", Branch = SkillBranch.Cryomancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "cryo_frost_nova", ToNodeId = "cryo_blizzard", Branch = SkillBranch.Cryomancy });
        AllLines.Add(new ConstellationLine { FromNodeId = "cryo_permafrost", ToNodeId = "cryo_blizzard", Branch = SkillBranch.Cryomancy });

        // Arcane lines
        AllLines.Add(new ConstellationLine { FromNodeId = "arcane_barrage", ToNodeId = "arcane_echo", Branch = SkillBranch.ArcaneMastery });

        // Vitality lines
        AllLines.Add(new ConstellationLine { FromNodeId = "vitality_hp", ToNodeId = "vitality_speed", Branch = SkillBranch.Vitality });
        AllLines.Add(new ConstellationLine { FromNodeId = "vitality_hp", ToNodeId = "vitality_magnet", Branch = SkillBranch.Vitality });
        AllLines.Add(new ConstellationLine { FromNodeId = "vitality_speed", ToNodeId = "vitality_mana_find", Branch = SkillBranch.Vitality });
        AllLines.Add(new ConstellationLine { FromNodeId = "vitality_magnet", ToNodeId = "vitality_mana_find", Branch = SkillBranch.Vitality });

        // Celestial bridges connecting central Arcane Core to the 4 quadrants
        AllLines.Add(new ConstellationLine { FromNodeId = "arcane_barrage", ToNodeId = "fireball", Branch = SkillBranch.ArcaneMastery });
        AllLines.Add(new ConstellationLine { FromNodeId = "arcane_barrage", ToNodeId = "chain_lightning", Branch = SkillBranch.ArcaneMastery });
        AllLines.Add(new ConstellationLine { FromNodeId = "arcane_barrage", ToNodeId = "ice_shard", Branch = SkillBranch.ArcaneMastery });
        AllLines.Add(new ConstellationLine { FromNodeId = "arcane_barrage", ToNodeId = "vitality_hp", Branch = SkillBranch.ArcaneMastery });
    }

    public void SyncWithGameState(GameState state)
    {
        foreach (var node in AllNodes)
        {
            if (state.MetaSkillLevels.TryGetValue(node.Id, out int lvl))
            {
                node.Level = lvl;
            }
            else
            {
                if (node.Id == "fireball" || node.Id == "arcane_barrage")
                {
                    node.Level = 1;
                    state.MetaSkillLevels[node.Id] = 1;
                }
                else
                {
                    node.Level = 0;
                }
            }
        }
    }

    public bool CanUnlock(SkillNode node, GameState state)
    {
        if (node.Level >= node.MaxLevel) return false;
        double cost = node.GetCostForNextLevel();
        if (state.Mana < cost) return false;

        if (!string.IsNullOrEmpty(node.PrerequisiteNodeId))
        {
            var prereq = AllNodes.FirstOrDefault(n => n.Id == node.PrerequisiteNodeId);
            if (prereq == null || prereq.Level <= 0) return false;
        }

        return true;
    }

    public bool PurchaseNode(string nodeId, GameState state)
    {
        var node = AllNodes.FirstOrDefault(n => n.Id == nodeId);
        if (node == null || !CanUnlock(node, state)) return false;

        double cost = node.GetCostForNextLevel();
        state.Mana -= cost;
        node.Level++;
        state.MetaSkillLevels[node.Id] = node.Level;

        return true;
    }

    public List<string> GetUnlockedSpellIds(GameState state)
    {
        SyncWithGameState(state);
        return AllNodes
            .Where(n => n.IsSpellUnlock && n.Level > 0 && !string.IsNullOrEmpty(n.AssociatedSpellId))
            .Select(n => n.AssociatedSpellId!)
            .ToList();
    }

    public void ApplyPermanentStatsToHero(GameState state, HeroEntity hero)
    {
        SyncWithGameState(state);

        int hpLvl = state.MetaSkillLevels.GetValueOrDefault("vitality_hp", 0);
        int spdLvl = state.MetaSkillLevels.GetValueOrDefault("vitality_speed", 0);
        int magLvl = state.MetaSkillLevels.GetValueOrDefault("vitality_magnet", 0);

        hero.MaxHealth = 100.0 + (hpLvl * 25.0);
        hero.CurrentHealth = hero.MaxHealth;
        hero.MoveSpeed = 180.0 * (1.0 + (spdLvl * 0.08));
        hero.MagnetRadius = 120.0 * (1.0 + (magLvl * 0.25));
    }
}
