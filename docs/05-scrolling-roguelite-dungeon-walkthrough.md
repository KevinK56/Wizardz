# Walkthrough: Scrolling Dungeon Roguelite & Meta Skill Tree Overhaul

## Overview
Successfully overhauled **Wizardz** on branch `feature/auto-battle-combat` into a **Top-Down Action Roguelite Bullet-Heaven Crawler** (inspired by *"The Fire Must Be Fed"* on Steam and *"Vampire Survivors"*), featuring:
1. **Direct WASD / Arrow Movement**: Free 2D navigation across a 2000x2000 stone dungeon map with smooth camera tracking.
2. **Auto-Firing Elemental Spells & Attack VFX**: Wizard auto-targets nearby monsters with staff thrust recoil, glowing projectile trails (Fireball, Chain Lightning, Ice Shards, Arcane Missiles), and impact explosion bursts.
3. **In-Run XP Gems & 3-Card Level-Up Draft**: Defeated monsters drop glowing gems pulled by your magnet radius; leveling up triggers an authentic 3-card spell upgrade modal on the fly.
4. **Player Health & Permadeath Run Loop**: HP bar with monster contact damage and invulnerability frames. Dying ends the run while preserving all banked Mana.
5. **Large Persistent Meta Skill Tree**: Removed the idle guild roster; players invest banked Mana into an expansive, branching elemental skill tree (Pyromancy, Electromancy, Cryomancy, Arcane, Vitality) to unlock new spells and permanently boost baseline stats.
6. **Floor Progression & 10th Floor Bosses**: Slain monsters unlock the descending staircase; every 10th floor features a Boss encounter with a dedicated health bar and guaranteed equipment loot.
7. **Authentic 16-Bit Chunky Pixel RPG Theme**: Google pixel font (`Press Start 2P`), double-beveled chunky pixel borders, wooden/parchment color scheme, and retro health meters.

---

## Changes Implemented

### 1. New Models (`Wizardz.Shared/Models`)
- [`SkillTree.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/SkillTree.cs): Meta-progression nodes across 5 branches (Pyromancy, Electromancy, Cryomancy, Arcane, Vitality).
- [`InRunSpellState.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/InRunSpellState.cs): In-run live active spell instances, cooldown calculations, and `DraftOption` model.
- [`DungeonMap.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/DungeonMap.cs): Scrolling world entities: `HeroEntity`, `MonsterEntity`, `XpGemEntity`, `SpellProjectile`, `ImpactVFX`, and `DungeonStairs`.
- [`GameState.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/GameState.cs): Integrated `MetaSkillLevels` and removed passive idle generation dependency.

### 2. New Services (`Wizardz.Shared/Services`)
- [`SkillTreeManager.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Services/SkillTreeManager.cs): Builds 16+ skill nodes, validates prerequisites, manages level purchases with Mana, and applies baseline stats to the hero.
- [`ScrollingDungeonEngine.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Services/ScrollingDungeonEngine.cs):
  - Direct WASD movement vector with boundary clamping.
  - Camera tracking centered on the Hero.
  - Monster swarm spawning outside the viewport perimeter.
  - Auto-casting elemental spells on cooldowns with staff casting recoil.
  - Projectile collision, splash damage, and explosion VFX.
  - Magnet collection of XP gems, leveling curve, and 3-card draft generation.
  - Floor 10 Boss encounters and descending stairs.

### 3. Interactive UI Components (`Wizardz.Shared/Components`)
- [`ScrollingDungeonCanvas.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/ScrollingDungeonCanvas.razor):
  - High-performance top-down viewport with hardware-accelerated CSS 3D translation.
  - In-run HUD: top XP progress bar (`Lv. X`), Hero HP meter, Floor badge, Kill counter, and Boss health banner.
  - Wizard sprite with 8-direction rotation, staff casting recoil, and invulnerability blinking.
  - Monsters with visible bodies, health meters, and hit flashes.
  - On-screen virtual D-Pad buttons for touch/mouse navigation.
- [`LevelUpDraftModal.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/LevelUpDraftModal.razor):
  - Retro 3-card draft popup with elemental borders, rank indicators (`Lv. 1 ➔ Lv. 2`), and descriptions.
- [`SpellSkillTree.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/SpellSkillTree.razor):
  - Expansive meta-progression skill tree screen with branch category navigation (Fire, Lightning, Frost, Arcane, Vitality), level pips, and upgrade buttons.
- [`GameOverModal.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/GameOverModal.razor):
  - Run summary showing Floors Reached, Monsters Slain, In-Run Level, Total Saved Mana, and Play Again button.
- [`Home.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Pages/Home.razor):
  - Main stage hosts `ScrollingDungeonCanvas`.
  - Right tab navigation: `📜 Skills`, `⚔️ Gear`, `🏛️ Academy`, `🌌 Astral`.
- [`app.css`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/wwwroot/app.css):
  - Loaded `'Press Start 2P'` Google pixel font.
  - Chunky double-beveled pixel borders (`border: 4px solid #5a3c26`), authentic retro palette (walnut `#160e0a`, leather `#241710`, parchment `#eedbb6`, and ruby red HP bars).

---

## Verification Results

### Automated Tests
- Ran `dotnet test Wizardz/Wizardz.Tests/Wizardz.Tests.csproj`:
```text
Passed!  - Failed: 0, Passed: 14, Skipped: 0, Total: 14, Duration: 144 ms - Wizardz.Tests.dll (net10.0)
```
Covered:
- WASD direct movement velocity and hero positioning
- Gem drops and magnet pickup into in-run XP
- Skill tree purchasing, prerequisites, and hero stat compounding
- Floor scaling and Floor 10 Boss encounters
- Multi-target spell cooldowns, equipment generation, and save roundtrips

### Multi-Platform Compilation
- Ran `dotnet build Wizardz.slnx`:
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
Targets: net10.0-windows, net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-web, net10.0-client
```
