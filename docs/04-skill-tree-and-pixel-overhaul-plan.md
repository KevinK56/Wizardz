# Implementation Plan: Elemental Skill Tree, Monster Visibility & Retro Pixel Overhaul

Redesign **Wizardz** to remove the idle guild system, implement an expansive **Interactive Skill Tree** (inspired by *"The Fire Must Be Fed"* on Steam) where players buy and level up elemental spells (Fireball, Chain Lightning, Ice Shards, Meteor), fix monster spawn coordinates and visibility, add visible wizard attack animations, and apply an authentic chunky retro pixel-art theme.

---

## User Review Required

> [!IMPORTANT]
> **Key Architectural & Gameplay Changes**:
> 1. **Removal of Idle Guild**: The `Tower Guild Roster` (apprentices, alchemists generating passive MPS) is completely removed.
> 2. **Large Interactive Skill Tree** (Inspired by *"The Fire Must Be Fed"*):
>    - All progression now centers on an expansive, branching **Spell & Wizard Skill Tree**.
>    - Four primary elemental branches:
>      - 🔥 **Pyromancy**: *Fireball* (exploding projectile) ➔ *Ignite* (DoT burn) ➔ *Flame Nova* ➔ *Meteor Shower*.
>      - ⚡ **Electromancy**: *Chain Lightning* (arcs between enemies) ➔ *Static Field* ➔ *Overload* ➔ *Thunderstorm*.
>      - ❄️ **Cryomancy**: *Ice Shard* (piercing ice) ➔ *Frost Nova* (freezes surrounding foes) ➔ *Blizzard*.
>      - 🔮 **Arcane Mastery**: *Arcane Barrage* (fast homing darts) ➔ *Spell Echo* (double-cast chance) ➔ *Singularity*.
>    - Each skill node has levels (e.g. 0/10), increasing damage, cooldown reduction, projectile counts, and elemental procs.
>    - In battle, **all unlocked spells auto-cast on their respective cooldowns**, firing spectacular animated projectiles across the dungeon!
> 3. **Monster Positioning & Visibility Fix**:
>    - Tighten spawn boundary to `X: 20%–80%` and `Y: 22%–78%`, preventing monsters from clipping offscreen.
>    - Enlarge monster sprites with bold pixel-art frames, distinct health bars, and animated idle marches.
> 4. **Wizard Attack Animations & Spell VFX**:
>    - Wizard casting recoil: staff thrust, casting flash aura at the tip.
>    - Unique animated spell projectiles: flaming fireballs, electric jagged bolts, crystalline ice spikes, and purple arcane stars.
>    - Impact explosions on contact (fire bursts, electric sparks, ice shards).
> 5. **Authentic 16-Bit Chunky Pixel Aesthetic**:
>    - Pixel font (`'Press Start 2P'`, `monospace`) with crisp text rendering.
>    - Chunky stepped pixel borders (`border: 4px solid #4a2f1b`, inset double bevels) replacing modern rounded pills.
>    - Parchment & dark oak textures for the entire interface.

---

## Proposed Architecture

```
Wizardz.Shared/
├── Models/
│   ├── SkillTree.cs            // SkillNode definition, SkillBranch (Fire, Lightning, Frost, Arcane), node dependencies
│   ├── ActiveSpellCast.cs      // Live active spells casting in combat (cooldowns, damage, VFX type)
│   ├── EquipmentItem.cs        // Weapon, Robe, Hat, Ring slots & gear
│   ├── DungeonEntity.cs        // Fixed monster positioning, speeds, animation states
│   └── GameState.cs            // Replaces Wizards roster with SkillTreeState & UnlockedSkills
├── Services/
│   ├── SkillTreeService.cs     // Manages skill unlocks, level ups, stat compounding, spell cooldown ticks
│   ├── DungeonCrawlerEngine.cs // Monster room bounds fix, multi-spell projectile simulation & impact VFX
│   └── GameEngine.cs           // Orchestrates skill tree + dungeon crawl + saves
└── Components/
    ├── TopDownDungeonView.razor// Arena with fixed monster bounds, casting recoil, spell projectiles & explosions
    ├── SpellSkillTree.razor    // Large interactive branching skill tree with node connections & tooltips
    ├── EquipmentInventory.razor// Chunky pixel gear paper doll & backpack
    └── Home.razor              // Navigation tabs: 📜 Skill Tree, ⚔️ Gear, 🏛️ Academy, 🌌 Astral
```

---

## Proposed Changes

### 1. Skill Tree Model & Service (`Wizardz.Shared/Models` & `Services`)

#### [NEW] `SkillTree.cs`
- `SkillNode`:
  - `Id`: e.g. `fire_ball`, `chain_lightning`, `ice_shard`, `meteor`
  - `Name`, `Description`, `Icon`, `Branch` (Fire, Lightning, Frost, Arcane)
  - `Level` (0 to MaxLevel, e.g. 10)
  - `MaxLevel`: int
  - `BaseCost`: double (Mana / Soul Essence)
  - `CostMultiplier`: double
  - `RequiredNodeId`: Prerequisite node ID on the branch
  - `CooldownSeconds`: Interval at which this spell automatically fires
  - `BaseDamage`, `DamagePerLevel`, `ProjectileCount`, `SpecialEffect`

#### [NEW] `SkillTreeService.cs`
- Manages the skill tree nodes:
  - Generates the default tree structure with 16+ elemental and mastery nodes.
  - Handles purchasing/upgrading nodes: checks mana affordability and prerequisites.
  - Ticks spell cooldowns and triggers spell casts in `DungeonCrawlerEngine`.

#### [MODIFY] `GameState.cs`
- Remove references to `Wizards` (the idle units).
- Add `Dictionary<string, int> SkillLevels` (persisted node levels).
- Update `CurrentMps` to represent passive elemental spell DPS.

---

### 2. Combat & Monster Positioning Fixes (`DungeonCrawlerEngine.cs`)

- **Monster Spawn Bounds**:
  - Confine monster spawn locations strictly between `X: 20%` and `80%`, `Y: 22%` and `78%`.
  - Prevent any monster from wandering beyond room walls.
- **Spell Firing System**:
  - Rather than a single generic attack, the engine checks all unlocked active spells from the skill tree (Fireball, Chain Lightning, Ice Shards, Arcane Barrage) and launches them according to their individual cooldowns!
  - `MagicProjectile` enhanced with `SpellType` (Fire, Lightning, Frost, Arcane), explosive radius, and chain bounces.
- **Impact Explosions & Hit Effects**:
  - Emits explosion entities (`ImpactVFX`) that render burst animations for 0.3s.

---

### 3. Visuals & Authentic Pixel UI (`app.css` & Components)

#### [NEW] `SpellSkillTree.razor`
- Interactive visual skill tree:
  - Displays branches: **🔥 Pyromancy**, **⚡ Electromancy**, **❄️ Cryomancy**, **🔮 Arcane**.
  - Connecting pixel lines indicating prerequisites.
  - Node card with retro pixel frame, level counter (`Lv. 3/10`), cost button, and stats preview.

#### [MODIFY] `TopDownDungeonView.razor`
- **Wizard Attack Animation**:
  - Staff thrust / recoil animation on cast (`hero-casting`).
  - Muzzle flash at wand tip.
- **Enhanced Monster Sprites**:
  - Larger sprites (2.5rem), distinct pixel bodies, animated walk cycle, health bar with chunky pixel border.
- **Animated Projectiles & Explosions**:
  - 🔥 Fireball: spinning flame orb leaving an ember trail, bursting into a pixel fire ring on hit.
  - ⚡ Lightning: electric jagged beam flashing directly to target and arcing.
  - ❄️ Ice Shard: crystalline icicle piercing through enemies.

#### [MODIFY] `app.css`
- Complete overhaul to authentic 16-bit pixel RPG:
  - Pixel font import (`Press Start 2P`).
  - Chunky double-bevel pixel borders (`border: 4px solid #5a3c26; box-shadow: inset -3px -3px 0 #180e08, inset 3px 3px 0 #805739;`).
  - Replace all smooth pill shapes with retro rectangular stone/wood buttons.
  - Pixelated particle effects.

---

## Verification Plan

### Automated Tests
- In [`Wizardz.Tests`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Tests/GameEngineTests.cs):
  1. Verify Skill Tree initialization, node dependency unlocking, and cost scaling.
  2. Verify elemental spells fire on their respective cooldowns.
  3. Verify monster spawn coordinates stay strictly inside `[20, 80]` boundaries.
  4. Verify save/load payload correctly serializes and restores skill tree levels and equipped gear.

### Manual Verification
- Run Web & Windows MAUI app:
  1. Verify monsters spawn fully inside the room and march visibly toward the wizard.
  2. Verify the Wizard animates with a visible staff thrust/casting recoil on attacks.
  3. Open the new **📜 Skill Tree** tab, buy and upgrade Fireball and Chain Lightning.
  4. Verify Fireballs explode and Lightning arcs between monsters with animated VFX.
  5. Verify the entire UI has a chunky retro 16-bit pixel RPG aesthetic with pixel fonts.
