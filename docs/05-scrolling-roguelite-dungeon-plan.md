# Implementation Plan: Top-Down Scrolling Dungeon Roguelite & Skill Tree Overhaul

Transform **Wizardz** into a **Top-Down Scrolling Action Roguelite Crawler** (inspired by *"The Fire Must Be Fed"* and *"Vampire Survivors"*), featuring direct WASD/arrow wizard movement, a smoothly scrolling dungeon map, in-run 3-card spell level-up drafts, XP gems, every 10th floor bosses, player HP & death loop, an expansive permanent meta-progression Skill Tree, equipment inventory, and an authentic 16-bit chunky pixel RPG aesthetic.

---

## User Review Required

> [!IMPORTANT]
> **Confirmed Design Blueprint (from `/grill-me` alignment)**:
> 1. **Direct Movement Control**: The wizard is directly navigated across the dungeon with **WASD / Arrow keys** (and touch virtual pad / click-to-move for mobile/desktop flexibility).
> 2. **Scrolling Dungeon Map**: An open dungeon map with a camera that smoothly tracks the wizard, populated with stone floor tiles, wall pillars, destructible crates, and randomly spawning treasure chests.
> 3. **Combat & In-Run Level-Up Draft Loop**:
>    - Unlocked spells automatically target and fire at the nearest enemies on cooldowns with rich attack animations (staff thrust recoil, glowing projectile trails, impact explosion bursts).
>    - Monsters drop glowing **XP/Mana gems** upon death.
>    - Collecting gems fills the **in-run XP bar**. Reaching 100% triggers a **LEVEL UP** popup offering a 3-choice spell/perk draft (e.g. *Unlock Fireball*, *Upgrade Chain Lightning: +1 Arc, +25% Damage*, *Frost Nova*, *+10% Movement Speed*).
> 4. **Floors & Every 10th Level Boss**:
>    - Clearing the monster wave/quota reveals the glowing descending stairway to proceed deeper.
>    - Every 10th floor is a dedicated **Boss Level** featuring a gigantic boss with high HP, menacing attacks, and a guaranteed high-tier equipment chest upon defeat.
> 5. **Player Health & Permadeath Run Loop**:
>    - The wizard has a visible Health Bar. Contact with monsters deals damage.
>    - If HP hits 0, the current dungeon run concludes, but **all collected Mana, Essences, and Equipment are permanently preserved** in your account.
> 6. **Large Persistent Skill Tree (Meta-Progression)**:
>    - The old idle tower guild is replaced entirely with an expansive, branching **Spell & Wizard Skill Tree** (Pyromancy, Electromancy, Cryomancy, Arcane Mastery, Vitality).
>    - Spending saved Mana permanently unlocks new spells for your in-run draft pool and enhances baseline stats (Max HP, Movement Speed, Magnet Radius, Cooldown Reduction, Area of Effect).
> 7. **Authentic 16-Bit Chunky Pixel RPG Theme**:
>    - True retro pixel-art styling: pixel font (`'Press Start 2P'`), double-beveled chunky pixel borders (`#4a2f1b`), wooden/stone textures, and segmented retro health bars.

---

## Technical Architecture

```
Wizardz.Shared/
├── Models/
│   ├── SkillTree.cs            // Meta-progression nodes (Fireball, Chain Lightning, Blizzard, Max HP, Magnet)
│   ├── InRunSpellState.cs      // Live spells active during the run (Level, Cooldown, Damage, Projectile Count)
│   ├── DungeonMap.cs           // Scrolling map entities: Wizard (world X/Y), Monsters, XP Gems, Obstacles, Chests
│   ├── EquipmentItem.cs        // Weapon, Robe, Hat, Ring slots & rarities
│   └── GameState.cs            // Replaces old Wizards roster with MetaSkillLevels, TotalMana, UnlockedEquipment
├── Services/
│   ├── ScrollingDungeonEngine.cs// Direct WASD movement, camera viewport offset, collision, monster AI swarm, XP drops
│   ├── SkillTreeManager.cs     // Meta tree unlocking, dependencies, stat compounding
│   └── GameEngine.cs           // Run lifecycle (Start Run, Level Up Draft, GameOver / Victory, Meta Hub)
└── Components/
    ├── ScrollingDungeonCanvas.razor // Real-time scrolling top-down viewport (Wizard, Monsters, Gems, VFX, Projectiles)
    ├── LevelUpDraftModal.razor     // Retro 3-card draft popup on in-run level up
    ├── SpellSkillTree.razor        // Sprawling meta-progression skill tree screen
    ├── EquipmentInventory.razor    // Chunky pixel gear loadout & backpack
    └── GameOverModal.razor         // Run summary screen with stats and Mana banked
```

---

## Detailed Implementation Steps

### 1. Data Models & Meta-Progression (`Wizardz.Shared/Models`)

#### [NEW] `SkillTree.cs`
- Meta-progression nodes across 5 branches:
  - 🔥 **Pyromancy**: Unlocks Fireball into run pool, increases burn duration, unlocks Meteor.
  - ⚡ **Electromancy**: Unlocks Chain Lightning, adds arcs, increases stun chance, unlocks Thunderstorm.
  - ❄️ **Cryomancy**: Unlocks Ice Shards, adds piercing, unlocks Frost Nova and Blizzard.
  - 🔮 **Arcane**: Unlocks Arcane Barrage, increases projectile speed, unlocks Spell Echo.
  - 🛡️ **Wizard Vitality**: Base Max HP (+20 per rank), Base Movement Speed (+5% per rank), Magnet Pickup Radius (+15% per rank).

#### [NEW] `InRunSpellState.cs`
- Tracks the active arsenal during a run:
  - `SpellId`, `CurrentLevel` (1 to 5), `CurrentCooldownRemaining`, `BaseCooldown`, `Damage`, `ProjectileCount`, `AreaRadius`.
- Contains the 3-card draft generator that offers valid upgrades from the unlocked pool.

#### [NEW] `DungeonMap.cs`
- `WorldEntity` with world coordinates (`X`, `Y` from `0` to `2000`):
  - `HeroEntity`: World position, velocity (`Vx`, `Vy`), facing angle, HP, MaxHP, Invulnerability timer after hit.
  - `MonsterEntity`: World position, velocity homing toward wizard, HP, damage, speed, sprite.
  - `XpGemEntity`: World position, value (XP & Mana), magnet homing state when near hero.
  - `DungeonObstacle`: Stone pillars, destructible crates.
  - `StairsEntity`: Spawns when floor quota is reached to descend to next floor.

#### [MODIFY] `GameState.cs`
- Remove the old `WizardUnit` list (the idle guild units).
- Persist:
  - `Dictionary<string, int> MetaSkillLevels` (levels purchased on the permanent skill tree).
  - `List<EquipmentItem> Inventory` and `Dictionary<EquipmentSlot, EquipmentItem> EquippedGear`.
  - `CurrentDungeonFloor` and `HighestDungeonFloor`.

---

### 2. Scrolling Dungeon Engine & Combat (`ScrollingDungeonEngine.cs`)

- **Direct WASD / Touch Movement**:
  - Handles keyboard input (`KeyW`, `KeyA`, `KeyS`, `KeyD`, arrow keys) and optional virtual joystick / mouse click vector.
  - Moves the Wizard in real-time with smooth acceleration and wall collision.
- **Camera Tracking**:
  - Viewport follows the Wizard, centering the camera while clamping at dungeon map bounds.
- **Monster Swarm AI**:
  - Spawns monsters around the perimeter of the screen (safely inside the world map, outside current viewport, then marching into view).
  - Swarms toward the Wizard's current position.
- **Auto-Casting Spells with Attack VFX**:
  - **Fireball**: Wizard casts with staff recoil; fireball shoots toward nearest enemy, exploding in a fiery AOE burst on impact.
  - **Chain Lightning**: Fires jagged lightning bolt striking the target and arcing across nearby monsters.
  - **Ice Shards**: Shoots high-speed crystalline shards in a frontal spread, piercing enemies.
  - **Frost Nova**: Triggers a radial freezing blast around the wizard pushing enemies back.
- **XP Gems & Level-Up Event**:
  - Monsters drop gems on death; gems are pulled toward the wizard when inside the `MagnetRadius`.
  - Reaching the XP threshold pauses combat and triggers `OnLevelUp(List<DraftOption> options)`.
- **Boss Encounter Every 10th Floor**:
  - Wave spawns pause; the Dungeon Boss enters with a dedicated Boss health bar and special attack patterns.

---

### 3. UI Components & Chunky 16-Bit Pixel Overhaul

#### [NEW] `ScrollingDungeonCanvas.razor` (Replaces old stage)
- 60 FPS / requestAnimationFrame / timer-driven HTML5 canvas or hardware-accelerated CSS pixel arena:
  - Smooth camera translation: `style="transform: translate({-CameraX}px, {-CameraY}px)"`.
  - Pixel-art stone tile grid with pillars and torches.
  - Wizard sprite with 8-direction facing, walk cycle bob, and staff casting recoil.
  - Large, distinct pixel monster sprites with health bars and hit flashes.
  - Animated projectiles and impact explosions.
  - In-run HUD: Wizard HP bar, XP bar at the top, Level counter, Active Floor badge, Mini-kill counter.

#### [NEW] `LevelUpDraftModal.razor`
- Authentic pixelated popup displaying 3 retro cards:
  - Card icon, spell title, current rank (`Lv. 1 ➔ Lv. 2`), detailed upgrade description (`+25% Damage, +1 Projectile`), and click-to-pick.

#### [NEW] `SpellSkillTree.razor`
- Expansive meta-progression skill tree screen:
  - Node network connecting the 5 branches with pixel line connectors.
  - Node icons with level badges (`3/5`), cost buttons, and tooltips.

#### [NEW] `GameOverModal.razor`
- Displays run recap: Floors Cleared, Monsters Slain, Mana Earned, Level Reached.
- "Visit Skill Tree" button and "Play Again" button.

#### [MODIFY] `app.css`
- True 16-bit retro pixel theme:
  - Pixel font `'Press Start 2P'` loaded.
  - Chunky double-beveled pixel borders (`border: 4px solid #4a2f1b; box-shadow: inset -4px -4px 0 #180e08, inset 4px 4px 0 #7b5337;`).
  - Warm walnut wood (`#180e08`, `#26170d`), dark leather (`#3b2415`), parchment gold (`#eedbb6`), and ruby red health bars.

---

## Verification Plan

### Automated Tests (`Wizardz.Tests/GameEngineTests.cs`)
1. **WASD Movement & Velocity**: Test hero movement math and speed bonuses from stats/skills.
2. **XP & In-Run Level Up**: Test monster gem drops, magnet pickup radius, XP leveling curve, and 3-card draft generation.
3. **Spell Auto-Casting & Damage**: Test Fireball, Chain Lightning, and Frost Nova cooldowns, projectile spawns, and damage math.
4. **Boss Floors**: Verify every 10th floor spawns the Boss and grants guaranteed equipment loot upon defeat.
5. **Player Damage & Death**: Test contact damage, invulnerability frames, game-over trigger, and mana retention.
6. **Skill Tree Meta-Progression**: Test node purchasing, prerequisite validation, and stat compounding across runs.

### Manual Verification
1. Run the MAUI Windows app / Web app.
2. Use WASD keys: verify the wizard moves smoothly across the scrolling dungeon floor and the camera follows.
3. Verify monsters spawn visibly in the map and march toward the wizard.
4. Watch the wizard auto-cast Fireballs and Lightning with staff thrusts and animated explosions.
5. Collect gems: verify the XP bar fills and the 3-card Level Up draft modal appears. Pick an upgrade and verify the spell becomes stronger immediately.
6. Reach Floor 10: fight the Boss, claim the equipment chest, equip gear in the Inventory.
7. Let the wizard take damage until HP reaches 0: verify Game Over screen appears, all collected Mana is saved, and you can open the Skill Tree to permanently unlock new abilities!
