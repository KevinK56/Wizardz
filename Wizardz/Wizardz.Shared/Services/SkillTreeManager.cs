using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public class SkillTreeManager
{
    public List<SkillNode> AllNodes { get; } = new();

    public SkillTreeManager()
    {
        InitializeTree();
    }

    private void InitializeTree()
    {
        // 1. Pyromancy Branch
        AllNodes.Add(new SkillNode
        {
            Id = "fireball",
            Name = "Fireball",
            Description = "Hurls an explosive sphere of fire that detonates upon impact.",
            Icon = "🔥",
            Branch = SkillBranch.Pyromancy,
            BaseCost = 50,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "fireball"
        });

        AllNodes.Add(new SkillNode
        {
            Id = "pyro_ignite",
            Name = "Ignite Infusion",
            Description = "+20% Fire spell damage and adds fiery burning trail.",
            Icon = "♨️",
            Branch = SkillBranch.Pyromancy,
            BaseCost = 120,
            MaxLevel = 5,
            PrerequisiteNodeId = "fireball",
            StatBonusPerLevel = 20.0
        });

        AllNodes.Add(new SkillNode
        {
            Id = "pyro_meteor",
            Name = "Meteor Shower",
            Description = "Calls blazing celestial meteors from the sky crushing enemy clusters.",
            Icon = "☄️",
            Branch = SkillBranch.Pyromancy,
            BaseCost = 350,
            MaxLevel = 5,
            PrerequisiteNodeId = "pyro_ignite",
            IsSpellUnlock = true,
            AssociatedSpellId = "meteor"
        });

        // 2. Electromancy Branch
        AllNodes.Add(new SkillNode
        {
            Id = "chain_lightning",
            Name = "Chain Lightning",
            Description = "Discharges electric arcs that jump between multiple nearby monsters.",
            Icon = "⚡",
            Branch = SkillBranch.Electromancy,
            BaseCost = 75,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "chain_lightning"
        });

        AllNodes.Add(new SkillNode
        {
            Id = "electro_overcharge",
            Name = "Overcharge",
            Description = "+10% Faster cooldown recovery for all elemental spells.",
            Icon = "🔋",
            Branch = SkillBranch.Electromancy,
            BaseCost = 150,
            MaxLevel = 5,
            PrerequisiteNodeId = "chain_lightning",
            StatBonusPerLevel = 10.0
        });

        AllNodes.Add(new SkillNode
        {
            Id = "electro_storm",
            Name = "Tempest Storm",
            Description = "Summons a roaming thunderstorm that automatically strikes foes.",
            Icon = "🌩️",
            Branch = SkillBranch.Electromancy,
            BaseCost = 400,
            MaxLevel = 5,
            PrerequisiteNodeId = "electro_overcharge",
            IsSpellUnlock = true,
            AssociatedSpellId = "storm"
        });

        // 3. Cryomancy Branch
        AllNodes.Add(new SkillNode
        {
            Id = "ice_shard",
            Name = "Ice Shards",
            Description = "Fires crystalline piercing shards of frost in a forward cone.",
            Icon = "❄️",
            Branch = SkillBranch.Cryomancy,
            BaseCost = 60,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "ice_shard"
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
            AssociatedSpellId = "frost_nova"
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
            AssociatedSpellId = "blizzard"
        });

        // 4. Arcane Mastery Branch
        AllNodes.Add(new SkillNode
        {
            Id = "arcane_barrage",
            Name = "Arcane Missiles",
            Description = "Rapidly fires homing arcane darts seeking out nearest enemies.",
            Icon = "🔮",
            Branch = SkillBranch.ArcaneMastery,
            BaseCost = 50,
            MaxLevel = 5,
            IsSpellUnlock = true,
            AssociatedSpellId = "arcane_barrage"
        });

        AllNodes.Add(new SkillNode
        {
            Id = "arcane_echo",
            Name = "Spell Echo",
            Description = "+8% chance per rank to cast any spell twice simultaneously.",
            Icon = "🌀",
            Branch = SkillBranch.ArcaneMastery,
            BaseCost = 200,
            MaxLevel = 5,
            PrerequisiteNodeId = "arcane_barrage",
            StatBonusPerLevel = 8.0
        });

        // 5. Vitality Branch (Base Stats)
        AllNodes.Add(new SkillNode
        {
            Id = "vitality_hp",
            Name = "Arcane Warding",
            Description = "+25 Base Max Health for all future dungeon runs.",
            Icon = "❤️",
            Branch = SkillBranch.Vitality,
            BaseCost = 40,
            MaxLevel = 10,
            StatBonusPerLevel = 25.0
        });

        AllNodes.Add(new SkillNode
        {
            Id = "vitality_speed",
            Name = "Wind Stride",
            Description = "+8% Movement speed to outmaneuver dungeon swarms.",
            Icon = "👟",
            Branch = SkillBranch.Vitality,
            BaseCost = 50,
            MaxLevel = 10,
            StatBonusPerLevel = 8.0
        });

        AllNodes.Add(new SkillNode
        {
            Id = "vitality_magnet",
            Name = "Aura Magnet",
            Description = "+25% Pickup radius to draw XP and Mana gems effortlessly.",
            Icon = "🧲",
            Branch = SkillBranch.Vitality,
            BaseCost = 45,
            MaxLevel = 10,
            StatBonusPerLevel = 25.0
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
            StatBonusPerLevel = 15.0
        });
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
                // Starter spells unlocked by default at Level 1: Fireball & Arcane Missiles
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
