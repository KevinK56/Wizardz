# Implementation Plan: Wizardz Idle Incremental Game

Create the foundational architecture and playable core for **Wizardz**, an idle incremental wizard guild game built on .NET 10 MAUI & Blazor Hybrid, featuring local persistence and cloud-save support.

---

## User Review Required

> [!IMPORTANT]
> **Cross-Platform Cloud Save Approach**:
> Because .NET MAUI targets Android, iOS, Windows, and Web, cloud storage SDKs vary (Google Drive / Play Games for Android, Apple iCloud / CloudKit for iOS). 
> 
> We will implement:
> 1. **Automated Local Persistence**: Instant, resilient local storage (using file storage in MAUI and `localStorage` in Web) with auto-save every 10 seconds and on app pause/exit.
> 2. **Cloud Sync Abstraction (`ICloudSaveService`)**: A standardized interface that handles cloud sync, conflict resolution (prompting the player if cloud vs local has different timestamps/progress), and export/import cloud-compatible save strings.
> 3. **Native Cloud Provider Hooks**: Provider templates for **Google Drive / Play Games** and **Apple iCloud**, alongside an immediate **Manual Cloud Backup / Transfer String** (like Cookie Clicker / Melvor Idle) so you can transfer your game between Android, iOS, Windows, and Browser immediately without requiring developer console credentials right now.

---

## Proposed Architecture & Components

```
Wizardz.Shared/
├── Models/
│   ├── GameState.cs             // Player stats, Mana, Essence, Astral Shards, Timestamps
│   ├── WizardUnit.cs            // Apprentices, Alchemists, Spellweavers, etc.
│   ├── Spell.cs                 // Active abilities (Arcane Surge, Time Warp, Transmute)
│   ├── Upgrade.cs               // Research upgrades, multipliers, prestige perks
│   └── SavePayload.cs           // Versioned envelope with checksum & timestamp
├── Services/
│   ├── GameEngine.cs            // Real-time tick loop, offline progress calculation, clicks
│   ├── ISaveStorage.cs          // Local storage contract
│   ├── ICloudSaveService.cs     // Cloud save & sync contract with conflict resolution
│   ├── LocalSaveStorage.cs      // Cross-platform local file/storage handler
│   └── CloudSaveManager.cs      // Cloud sync orchestration & manual cloud transfer strings
├── Components/
│   ├── ArcaneOrb.razor          // Interactive orb with floating click numbers & particle flair
│   ├── WizardRoster.razor       // Wizard purchase list with bulk buy (1x, 10x, Max)
│   ├── SpellbookBar.razor       // Active spells with cooldowns & animation
│   ├── UpgradeShop.razor        // Academy research and multipliers
│   ├── AstralAscension.razor    // Prestige reset modal & celestial tree
│   └── CloudSaveModal.razor     // Cloud sync status, manual backup/restore, conflict modal
└── Pages/
    └── Home.razor               // Main integrated dashboard for Wizardz
```

---

## Proposed Changes

### 1. Game State & Economy Models (`Wizardz.Shared/Models`)

#### [NEW] `GameState.cs`
- Currencies: `Mana`, `ArcaneEssence`, `AstralShards`, `LifetimeManaEarned`.
- Multipliers: `ClickMultiplier`, `GlobalProductionMultiplier`.
- Collections: `Wizards`, `Upgrades`, `ActiveBuffs`.
- Timestamps: `LastSaveTimeUtc`, `LastTickTimeUtc` for calculating offline earnings upon returning.

#### [NEW] `WizardUnit.cs`
- Tiered wizards:
  - **Novice Apprentice**: Low cost, 1 MPS (Mana Per Second).
  - **Alchemical Scholar**: 8 MPS + yields occasional reagent bonus.
  - **Spellweaver**: 45 MPS.
  - **Pyromancer**: 260 MPS.
  - **Void Invoker**: 1,600 MPS.
  - **Archmage Council**: 12,000 MPS.
- Exponential cost scaling formula: $\text{Cost} = \text{BaseCost} \times 1.15^{\text{Count}}$.

#### [NEW] `Spell.cs`
- Active abilities with cooldowns:
  - **Arcane Surge**: 5x Click & MPS for 20 seconds (Cooldown: 90s).
  - **Time Warp**: Instantly grants 15 minutes of passive mana production (Cooldown: 180s).
  - **Transmutation**: Transmutes 20% of current Mana into Arcane Essence (Cooldown: 60s).

---

### 2. Game Engine & Offline Progression (`Wizardz.Shared/Services`)

#### [NEW] `GameEngine.cs`
- High-precision ticker (10 Hz for smooth UI counter interpolation, delta-time math).
- Offline progression calculator: When the game launches, calculates time elapsed since `LastSaveTimeUtc`, simulates passive wizard generation up to a configurable cap (e.g. 24 hours), and displays an "Offline Gains" welcome modal.
- Active spell cooldown & buff duration timers.

---

### 3. Local Storage & Cloud Sync (`Wizardz.Shared/Services`)

#### [NEW] `ISaveStorage.cs` & `ICloudSaveService.cs`
- `ISaveStorage`: `SaveAsync(GameState state)`, `LoadAsync()`, `ExportSaveStringAsync()`, `ImportSaveStringAsync(string code)`.
- `ICloudSaveService`:
  - `CheckCloudStatusAsync()`: Checks whether cloud is connected (Google Play / iCloud / Cloud Account).
  - `SyncAsync(GameState localState)`: Compares local timestamp and lifetime mana with cloud copy.
  - Detects conflict and returns `SyncResult` (Success, LocalUpdated, CloudNewerPromptRequired).

#### [NEW] Platform Implementations
- In `Wizardz` (MAUI): Native file-based save in `FileSystem.AppDataDirectory` (isolated, persistent per OS).
- In `Wizardz.Web` / Blazor: Browser `localStorage` via JS runtime fallback.

---

### 4. Atmospheric Game UI (`Wizardz.Shared`)

#### [NEW] `ArcaneOrb.razor`
- Glowing central rune orb that pulses when clicked.
- Spawns floating damage/mana numbers on click with smooth CSS animations.

#### [NEW] `WizardRoster.razor`, `SpellbookBar.razor`, `AstralAscension.razor`
- Compact, responsive UI tabs fitting both mobile (Android/iOS phone aspect ratio) and desktop/web widescreen layout.
- Bulk buy buttons: `x1`, `x10`, `x100`, `Max`.
- Ascension screen detailing how many Astral Shards you will claim on reset.

#### [MODIFY] `Wizardz.Shared/Pages/Home.razor`
- Replace default template with the complete Wizardz dashboard layout, top status bar (resources, MPS, offline gains), center orb view, and bottom/side action panels.

---

## Verification Plan

### Automated Tests / Builds
- Build all targets cleanly via `dotnet build Wizardz.slnx`.
- Verify runtime compilation without warnings.

### Manual & Interactive Verification
- Run the app (Windows Desktop or Web) to verify:
  1. Clicking the Arcane Focus increments Mana and displays floating click gains.
  2. Purchasing Novice Apprentices and higher tiers updates MPS and generates mana automatically every second.
  3. Activating spells applies temporary buffs and triggers cooldown timers.
  4. Closing and reopening the app restores saved state and calculates offline gains accurately.
  5. Cloud Save dialog allows exporting and importing save strings and simulates cloud sync checks.
