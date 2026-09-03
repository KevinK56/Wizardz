# Implementation Plan: Top-Down Pixel Dungeon Crawler & Equipment System

Transform **Wizardz** into a **top-down 2D retro dungeon crawler** with procedural dungeon levels, equipment gear slots, random treasure chests, boss floors every 10th level, and a warm **brown pixelated RPG UI aesthetic**.

---

## User Review Required

> [!IMPORTANT]
> **Key Design Shifts**:
> 1. **Top-Down Dungeon Crawler Arena**: Replaces the central stage with a top-down tile-based dungeon chamber where your Wizard explores, auto-targets, and casts magical projectiles in 360° at patrolling monsters.
> 2. **Dungeon Level Progression & Bosses**: Levels advance deeper into the dungeon. Levels 1–9 feature dungeon rooms with creeping enemies and random chests; every **10th level is an ominous Boss Chamber**.
> 3. **Equipment & Inventory System**: 4 equippable gear slots (**Staff/Wand**, **Robe**, **Wizard Hat**, **Runic Ring**) with RPG rarities (*Common, Uncommon, Rare, Epic, Legendary*).
> 4. **Random Treasure Chests**: Wooden, Gold, and Arcane Mystery Chests spawn randomly in dungeon rooms during exploration and burst open with equipment and mana when clicked or approached.
> 5. **Warm Brown Pixelated UI**: Replaces the neon sci-fi purple with a classic 16-bit fantasy palette: dark walnut wood (`#2c1b12`), aged leather (`#4a2f1b`), parchment gold (`#c8963e`), pixel borders, and chunky retro styling.

---

## Architecture & Data Models

```
Wizardz.Shared/
├── Models/
│   ├── DungeonLevel.cs         // Dungeon themes (Catacombs, Crypt, Magma, Void), tile layouts, level numbering
│   ├── DungeonEntity.cs        // Wizard Hero, Monsters, Projectiles, Floating loot
│   ├── EquipmentItem.cs        // Gear slots (Weapon, Robe, Hat, Ring), stats, rarities
│   ├── TreasureChest.cs        // Random chest spawns (Wood, Gold, Arcane), loot tables
│   ├── GameState.cs            // Updated to store Inventory, EquippedGear, DungeonFloor, Stats
│   └── WizardUnit.cs / Spell.cs// Tower Guild & Grimoire preserved as guild backup
├── Services/
│   ├── DungeonCrawlerEngine.cs // Top-down real-time physics/tick, monster AI patrol, projectile collision, chest drop math
│   ├── EquipmentGenerator.cs   // Procedural loot drop generator based on current dungeon depth
│   ├── GameEngine.cs           // Orchestrates idle spire + active dungeon crawl + save systems
│   └── IGameNotificationService.cs // SignalR real-time event distribution
└── Components/
    ├── TopDownDungeonView.razor// The interactive top-down pixel canvas/arena with animated wizard & monsters
    ├── EquipmentInventory.razor// Gear slots, equipment tooltips, compare & equip
    ├── ChestLootModal.razor    // Pixelated pop-up when cracking open a treasure chest
    ├── DungeonHud.razor        // Level progress (Floor X/10), mini-map / room indicator, boss alert
    └── WizardRoster.razor      // Re-styled with brown pixelated borders & guild mechanics
```

---

## Proposed Changes

### 1. New Models & Progression Systems (`Wizardz.Shared/Models`)

#### [NEW] `EquipmentItem.cs`
- **Slot Types**: `Weapon` (Wand/Staff), `Armor` (Robe), `Head` (Wizard Hat), `Accessory` (Runic Ring).
- **Rarities**: `Common` (Gray), `Uncommon` (Green), `Rare` (Blue), `Epic` (Purple), `Legendary` (Golden Orange).
- **Stats**:
  - Attack Damage (`+ATK`)
  - Attack Speed / Cast Rate (`+SPD`)
  - Critical Hit Chance & Multiplier (`+CRIT`)
  - Magic Find / Bonus Mana Drops (`+FIND`)
- Upgrade & Scrap: Duplicate or lower-tier items can be disassembled into Arcane Dust to upgrade equipped gear.

#### [NEW] `TreasureChest.cs`
- Spawns randomly in rooms with a percentage chance per room clear or timer.
- **Chest Tiers**:
  - *Wooden Cask*: Small Mana cache + chance of Common/Uncommon gear.
  - *Gilded Iron Chest*: Large Mana + guaranteed Rare item.
  - *Arcane Runic Chest*: High Essence + chance of Epic/Legendary equipment.
- Interactive: Player can click/tap to unlock immediately, or the wizard automatically claims it upon clearing room enemies.

#### [NEW] `DungeonLevel.cs` & `DungeonTheme`
- Biome Themes:
  - **Floors 1–10: Mossy Catacombs** (Stone bricks, vine moss, slimes, skeletons, bats. Boss: *Bone Golem Lord*).
  - **Floors 11–20: Sunken Crypt** (Dark slate, candles, wraiths, ghouls, necro-cultists. Boss: *Lich Sovereign*).
  - **Floors 21–30: Magma Caldera** (Volcanic basalt, lava cracks, flame elementals, hellhounds. Boss: *Pyroclast Drake*).
  - **Floors 31–40: Astral Vault** (Runic star-tiles, arcane void-spiders, cosmic sentinels. Boss: *Eldritch Titan*).
- Floor 10 Boss mechanism: Boss health bar at top of dungeon screen, ominous boss music/ambient pulses, guaranteed Rare+ equipment chest drop upon victory!

#### [MODIFY] `GameState.cs`
- Add:
  - `CurrentDungeonFloor`: int (default 1)
  - `HighestDungeonFloor`: int
  - `EquippedGear`: Dictionary of slot to `EquipmentItem`
  - `Inventory`: List of `EquipmentItem`
  - `ActiveChest`: Live chest if one is currently in the room

---

### 2. Top-Down Dungeon Engine (`Wizardz.Shared/Services`)

#### [NEW] `DungeonCrawlerEngine.cs`
- Controls the top-down simulation:
  - Wizard position (`X`, `Y`), facing angle, target locking.
  - Monster waves approaching the wizard: each monster has position, speed, health, and attacks.
  - Magical Projectiles: Wizard fires homing or straight spell bolts towards the closest enemy.
  - Direct Player Interaction: Tapping on any enemy casts an instant player Thunder Strike or Arcane Beam dealing high click damage.
  - Room Transitions: When all monsters in a room are defeated, the exit portal/stairway opens and the Wizard descends to the next floor.

---

### 3. Brown Pixelated UI & Visual Components (`Wizardz.Shared`)

#### [NEW] `TopDownDungeonView.razor`
- Top-down room view featuring:
  - Pixelated stone floor tiles, corner walls, torch sconces with flickering animated flames.
  - Pixel-art Wizard Hero sprite with animated walk/cast cycles, glowing wand tip, and robe colors corresponding to equipped robe.
  - Enemy sprites with pixel outlines, health meters, and knockback hit flashes.
  - Animated spell projectiles (sparking magic orbs, fire bolts, lightning bolts).
  - Chest entity sitting in the chamber with glowing aura when spawned.

#### [NEW] `EquipmentInventory.razor`
- Classic RPG paper-doll view showing:
  - 4 Equipment Slots on left/right of the Wizard sprite.
  - Inventory grid with item icons and rarity borders.
  - Click-to-equip and item stat tooltip comparison (e.g. `+14 ATK (+4) | +2% CRIT`).

#### [MODIFY] `Wizardz.Shared/wwwroot/app.css`
- **Brown Pixelated Theme**:
  - Deep walnut/parchment color variables: `--bg-wood: #20140e`, `--bg-stone: #2e221b`, `--border-pixel: #593d28`, `--parchment-light: #e6d3a3`, `--gold-accent: #d4a03e`.
  - Chunky retro borders (`border: 3px solid #593d28; box-shadow: inset -2px -2px 0px #1a100a, inset 2px 2px 0px #805739;`).
  - Pixel font styling (`image-rendering: pixelated; font-family: 'Courier New', monospace, sans-serif; font-weight: bold;`).
  - Retro pixel chest opening animation.

#### [MODIFY] `Home.razor`
- Main stage becomes `TopDownDungeonView`.
- Tab bar styled with wooden pixel buttons:
  - ⚔️ Dungeon / Hero Gear (`EquipmentInventory`)
  - 🧙 Tower Guild (`WizardRoster`)
  - 🏛️ Academy Research (`UpgradeShop`)
  - 🌌 Astral Gate (`AstralAscension`)

---

## Verification Plan

### Automated Tests
- In [`Wizardz.Tests`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Tests/GameEngineTests.cs):
  1. Test procedural equipment generation and stat rolling per floor.
  2. Test equipping, unequipping, and stat calculation (damage, crit, attack speed).
  3. Test random chest drop rates and loot opening.
  4. Test dungeon floor progression and 10th level boss encounter logic.
  5. Test save/load roundtrip with inventory and equipped gear.

### Manual Verification
- Run Web & MAUI Windows app:
  1. Inspect the top-down pixel dungeon arena: verify wizard sprite, stone walls, floor tiles, and torches.
  2. Verify wizard auto-targets and casts projectiles at spawning enemies.
  3. Verify clicking on enemies triggers interactive lightning strikes.
  4. Verify random chests spawn, can be clicked open, and drop gear into inventory.
  5. Verify opening inventory, equipping new wands/robes, and seeing stats increase in real-time.
  6. Verify reaching Floor 10 triggers the Boss fight with boss health bar and guaranteed loot.
  7. Verify the brown pixelated RPG aesthetic across all panels and buttons.
