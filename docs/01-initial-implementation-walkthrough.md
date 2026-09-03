# Walkthrough: Wizardz Idle Incremental Game (1st Implementation)

We have implemented the complete foundational architecture and playable core for **Wizardz**, an idle incremental game based on Option A (Arcane Spire & Wizard Guild) with automated local persistence, cloud-save sync support, and cross-platform readiness.

---

## Key Changes & Architecture

### 1. Documentation & Repository
- Saved the design plan to [`docs/01-initial-implementation-plan.md`](file:///c:/Dev/TheApp/Wizardz/docs/01-initial-implementation-plan.md).
- Created a standard `.gitignore` and committed all initial deliverables to Git (`master` branch).

### 2. Game Data Models (`Wizardz.Shared/Models`)
- [`GameState.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/GameState.cs): Central reactive model tracking Mana, Arcane Essence, Astral Shards, Lifetime Mana, Total Clicks, active buffs, and timestamps.
- [`WizardUnit.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/WizardUnit.cs): Units with exponential cost scaling ($Cost = BaseCost \times 1.15^{Count}$), geometric bulk-buy math (`x1`, `x10`, `x100`, `Max`), and passive MPS generation.
  - *Novice Apprentice, Alchemical Scholar, Arcane Spellweaver, Crimson Pyromancer, Void Invoker, Grand Archmage Council*.
- [`Spell.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/Spell.cs): Active spells with live cooldowns and duration ticks (*Arcane Surge*, *Temporal Warp*, *Transmutation*).
- [`Upgrade.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/Upgrade.cs): Multi-currency research upgrades targeting units, clicks, and global MPS.
- [`SavePayload.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Models/SavePayload.cs): Checksum-verified JSON and Base64 export/import container.

### 3. Engine & Persistence (`Wizardz.Shared/Services`)
- [`GameEngine.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Services/GameEngine.cs):
  - 10 Hz tick loop with delta-time calculation for smooth resource progression.
  - Offline progression math: calculates time elapsed since last save (up to 24h) and grants offline mana upon return.
  - Auto-save every 10 seconds.
  - Real-time affordability dispatching via SignalR notifications whenever mana is earned or spent.
- [`GameHub.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Web/Hubs/GameHub.cs) & [`SignalRNotificationService.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Services/SignalRNotificationService.cs):
  - SignalR Hub hosted at `/hubs/game` in ASP.NET Core with automatic reconnecting client.
  - Broadcasts `AffordabilityUpdated` and `StateUpdated` events to components.
  - Seamless fallback for offline MAUI execution without server dependency.
- [`LocalSaveStorage.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Services/LocalSaveStorage.cs): Dual-mode local persistence (native file system for MAUI desktop/mobile and browser `localStorage` for Web).
- [`CloudSaveService.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Services/CloudSaveService.cs): Standardized cloud interface supporting Google Drive and Apple iCloud, conflict detection (prompting user if cloud vs local has different timestamps/progress), and forced sync/download.
- [`ServiceCollectionExtensions.cs`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Services/ServiceCollectionExtensions.cs): Single `.AddWizardzGame()` extension method used by MAUI, Web, and Client.

### 4. Atmospheric UI & Components (`Wizardz.Shared/Components` & `Pages`)
- [`ArcaneOrb.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/ArcaneOrb.razor): Animated central orb with glowing rune rotation, pulse, and dynamic floating mana gain numbers on click.
- [`WizardRoster.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/WizardRoster.razor): Unit hiring interface with bulk-buy selector (`x1`, `x10`, `x100`, `Max`) and real-time affordability updates.
- [`SpellbookBar.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/SpellbookBar.razor): Visual spell slots with cooldown gauge overlays and active duration badges.
- [`UpgradeShop.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/UpgradeShop.razor): Research laboratory with milestone requirements.
- [`AstralAscension.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/AstralAscension.razor): Prestige view showing lifetime mana and claimable Astral Shards.
- [`CloudSaveModal.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/CloudSaveModal.razor): Cloud sync manager supporting provider selection (Google Drive / Apple iCloud), conflict comparison modal, and copyable save codes.
- [`OfflineGainsModal.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/OfflineGainsModal.razor): Welcome back dialog greeting players with their offline earnings.
- [`Home.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Pages/Home.razor): Integrated responsive dashboard with status bar and toast notifications.
- [`app.css`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/wwwroot/app.css): Dark fantasy styling with glowing purples, cyans, golds, and responsive mobile/desktop layout.

### 5. Settings, Auto-Save Configuration & UX Improvements
- [`SettingsModal.razor`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Shared/Components/SettingsModal.razor):
  - Consolidated settings dialog opened from the header `⚙️ Settings` button.
  - **Auto-Save Intervals**: Configurable local auto-save (default: 2 minutes) and cloud auto-save (default: 5 minutes) saved in state.
  - **Cloud Save & Sync**: Integrated Google Drive and Apple iCloud providers, manual code export/import, and conflict resolution.
  - **Danger Zone**: Reset game button guarded by an explicit destructive action confirmation dialog.
- **Remembered Buy Options**:
  - `WizardRoster.razor` stores and remembers the active multiplier (`x1`, `x10`, `x100`, `Max`) across tabs and sessions via `Engine.SetBuyQuantity()`.
- **Floating Toast Bubble**:
  - Replaced full-width notification banner with an unobtrusive, floating toast bubble in the bottom-right that pops in smoothly and leaves after 2.5s.

---

## Verification Results

### Automated Tests
Added [`Wizardz.Tests`](file:///c:/Dev/TheApp/Wizardz/Wizardz/Wizardz.Tests/GameEngineTests.cs) covering all game mechanics, SignalR notifications, buy preference persistence, and auto-save timer configuration:
```text
Passed!  - Failed: 0, Passed: 9, Skipped: 0, Total: 9, Duration: 79 ms - Wizardz.Tests.dll (net10.0)
```

### Multi-Target Compilation
Compiled all platforms via `dotnet build Wizardz.slnx`:
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
Targets: net10.0-windows, net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-web, net10.0-client
```

### Git Commit
Committed to local master branch:
```text
[master 974c93c] Implement Wizardz idle game foundation: models, 10Hz engine, offline progression, local & cloud save, dark fantasy UI, and unit tests
 29 files changed, 3556 insertions(+), 387 deletions(-)
```

---

## How to Run & Test the Game

You can launch either the MAUI Windows app or the Web app:

**To run the Web version in browser:**
```powershell
dotnet run --project Wizardz/Wizardz.Web
```

**To run the Windows Desktop MAUI app:**
```powershell
dotnet run --project Wizardz/Wizardz -f net10.0-windows10.0.19041.0
```
