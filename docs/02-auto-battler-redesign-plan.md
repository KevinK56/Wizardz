# Implementation Plan: Wizard Auto-Battler & Stage Progression System

Overhaul the center stage of **Wizardz** from the clickable orb into an animated **Wizard Hero Auto-Battler Arena** where a customizable Wizard Hero fights progressively stronger monsters through levels and stages, with active clicking, boss challenges, and hero incremental upgrades.

---

## User Review Required

> [!IMPORTANT]
> **Core Redesign Summary**:
> - **From Orb to Arena**: The central `ArcaneOrb` is transformed into an animated **Arcane Battle Arena**.
> - **The Wizard Hero**: Features an animated Wizard Hero sprite on the left casting auto-spells (magic missiles, arcane bolts) toward oncoming monsters on the right.
> - **Progressive Monster Stages**: Monsters scale in HP, level, and rewards across distinct biomes (e.g. *Whispering Forest*, *Ancient Crypt*, *Infernal Caldera*, *Celestial Rift*). Every 10th enemy is a **Boss** with a timed encounter.
> - **Active Clicking Preserved**: Tapping/clicking the monster or battlefield casts direct player spell strikes (lightning bolts / arcane strikes) with impact numbers, so active clicking directly helps defeat bosses and clears stages faster.
> - **Hero Upgrades**: Mana can be invested into the Wizard Hero's stats (Attack Power, Attack Speed, Critical Chance/Damage, Spell Amplification) alongside the Tower Guild roster.

---

## Proposed Architecture & Data Models

### 1. New & Modified Models (`Wizardz.Shared/Models`)

#### [NEW] `Monster.cs`
- Properties:
  - `Name`, `Emoji`, `Level`, `StageNumber`, `MaxHealth`, `CurrentHealth`, `ManaReward`, `EssenceReward`, `IsBoss`, `BiomeType`.
- Biomes / Zones:
  - **Zone 1: Whispering Woods** (Forest Slimes, Goblin Apprentices, Wolf Spirits, Treant Boss)
  - **Zone 2: Sunken Catacombs** (Skeletons, Wandering Wraiths, Crypt Ghouls, Lich Sovereign Boss)
  - **Zone 3: Magma Caverns** (Flame Imps, Fire Drakes, Lava Golems, Pyroclast Dragon Boss)
  - **Zone 4: Celestial Void** (Astral Shards, Cosmic Anomalies, Void Walkers, Astral Titan Boss)

#### [NEW] `HeroStats.cs`
- Hero combat attributes:
  - `Level`, `AttackDamage`, `AttacksPerSecond` (auto-battle pace), `CriticalChance` (0-100%), `CriticalMultiplier`, `ClickDamageBonus`.
  - Upgrade levels and formulas for each stat.

#### [MODIFY] `GameState.cs`
- Add:
  - `HeroStats Hero`: Hero stats & level.
  - `CurrentStage`: Current stage level (e.g. 1 to $\infty$).
  - `HighestStageUnlocked`: Tracks progression across ascensions.
  - `CurrentMonster`: Live monster state with current HP.
  - `BossTimerRemaining`: 30-second countdown during boss battles.
  - `IsBossFailed`: Auto-farms preceding monster if boss timer runs out, with a "Challenge Boss" button.

---

### 2. Engine & Combat Loop (`Wizardz.Shared/Services`)

#### [MODIFY] `GameEngine.cs`
- **Combat Tick Loop**:
  - Wizard Hero auto-attacks based on `AttacksPerSecond` delta time.
  - Deals `HeroAttackDamage` (with crit roll) to `CurrentMonster`.
  - Active screen clicks trigger immediate `ClickDamage` to the monster.
- **Defeat & Spawning**:
  - When monster HP reaches 0:
    - Awards Mana & Essence.
    - If non-boss: increments monster index (1 to 10).
    - If monster 10 (Boss): advances to next Stage/Zone and resets boss timer!
    - If boss timer hits 0 before defeat: retreats to stage 9 for farming until the player challenges the boss again.
- **Offline Combat Calculations**:
  - Offline gains now account for stage completion and monster clear rates based on hero damage output.

---

### 3. Visuals & Interactive Components (`Wizardz.Shared/Components`)

#### [NEW] `BattleArena.razor` (Replaces `ArcaneOrb.razor` on main stage)
- **Battlefield Visuals**:
  - Responsive arena with biome theme backdrops and particle weather (floating embers, arcane motes, leaves).
- **Animated Wizard Hero Sprite**:
  - CSS-animated pixel-art / vector wizard: robe fluttering, hovering animation, staff casting motion, magical charge aura.
  - Projectile launcher: animated magic missiles flying across the screen on each auto-attack.
- **Animated Monster Sprite**:
  - Monster sprites that react to hits (hit flash, bounce, death fade).
  - Health bar with current/max HP and boss icon.
  - Stage indicator header with mini-progress dots (e.g., `Stage 1-7 [●●●●●●●○○○]`).
- **Interactive Tapping / Impact Popups**:
  - Clicking spawns spell burst effects and floating damage numbers right over the monster.

#### [NEW] `HeroUpgradePanel.razor`
- New management tab or collapsible drawer alongside Tower Guild:
  - Upgrade **Staff Power** (Attack Damage).
  - Upgrade **Channeling Speed** (Attack Rate).
  - Upgrade **Arcane Precision** (Crit Chance & Crit Damage).
  - Upgrade **Spell Force** (Click Strike Power).

#### [MODIFY] `Home.razor`
- Host `BattleArena.razor` on the main stage.
- Add "Hero" tab in the right management panel alongside "Guild", "Academy", and "Astral Gate".

---

## Verification Plan

### Automated Tests
- Extend [`Wizardz.Tests`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Tests/GameEngineTests.cs) with tests for:
  1. Monster spawning, health scaling with stages, and reward calculation.
  2. Hero auto-attack damage application and critical strikes.
  3. Active click damage to monsters.
  4. Stage advancement upon defeating bosses and boss timer expiration logic.
  5. Hero stat upgrades and cost formulas.

### Manual Verification
- Run the Web or MAUI app:
  1. Verify Wizard Hero sprite animates and auto-casts projectiles at monsters.
  2. Verify tapping monsters deals instant click damage with floating numbers.
  3. Verify monsters take damage, die, yield mana, and advance stages 1 through 10.
  4. Verify Stage 10 Boss encounter with 30s timer and zone completion.
  5. Verify upgrading hero stats makes auto-attacks hit faster and harder.
