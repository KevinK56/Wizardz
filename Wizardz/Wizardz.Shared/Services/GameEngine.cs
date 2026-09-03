using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public class GameEngine : IDisposable
{
    private readonly ISaveStorage _saveStorage;
    private readonly ICloudSaveService _cloudSaveService;
    private readonly IGameNotificationService _notificationService;
    private readonly PeriodicTimer _tickTimer;
    private readonly CancellationTokenSource _cts = new();
    private Task? _timerTask;

    private double _localAutoSaveCounterSeconds = 0;
    private double _cloudAutoSaveCounterSeconds = 0;

    public GameState State { get; private set; }
    public bool IsInitialized { get; private set; } = false;

    // Events for reactive UI updates
    public event Action? OnStateChanged;
    public event Action<double>? OnManaClicked;
    public event Action<string>? OnNotification;
    public event Action<double, TimeSpan>? OnOfflineGainsCalculated;

    public GameEngine(ISaveStorage saveStorage, ICloudSaveService cloudSaveService, IGameNotificationService notificationService)
    {
        _saveStorage = saveStorage;
        _cloudSaveService = cloudSaveService;
        _notificationService = notificationService;
        State = GameState.CreateDefault();
        _tickTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100)); // 10 ticks per sec
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        await _notificationService.InitializeAsync();

        // Load persisted state
        var loaded = await _saveStorage.LoadStateAsync();
        if (loaded != null)
        {
            State = loaded;
            CalculateOfflineGains();
        }
        else
        {
            State = GameState.CreateDefault();
        }

        IsInitialized = true;
        State.LastTickTimeUtc = DateTime.UtcNow;

        // Start game loop
        _timerTask = RunGameLoopAsync(_cts.Token);
        OnStateChanged?.Invoke();
    }

    private void CalculateOfflineGains()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - State.LastSaveTimeUtc;

        // Offline progress if away for more than 10 seconds (capped at 24 hours)
        if (elapsed.TotalSeconds >= 10.0 && State.CurrentMps > 0)
        {
            double maxOfflineSeconds = Math.Min(elapsed.TotalSeconds, 24 * 3600);
            // 80% efficiency while offline
            double earned = State.CurrentMps * maxOfflineSeconds * 0.80;

            State.Mana += earned;
            State.LifetimeMana += earned;
            State.LastTickTimeUtc = now;

            OnOfflineGainsCalculated?.Invoke(earned, elapsed);
            OnNotification?.Invoke($"Welcome back! Your wizards generated {FormatNumber(earned)} Mana while you were away.");
        }
    }

    private async Task RunGameLoopAsync(CancellationToken ct)
    {
        DateTime previousTime = DateTime.UtcNow;

        try
        {
            while (await _tickTimer.WaitForNextTickAsync(ct))
            {
                DateTime currentTime = DateTime.UtcNow;
                double deltaSeconds = (currentTime - previousTime).TotalSeconds;
                previousTime = currentTime;

                // Safety clamp on delta time
                if (deltaSeconds > 1.0) deltaSeconds = 1.0;

                Tick(deltaSeconds);

                // Local Auto save (default 2 minutes or user configured)
                if (State.LocalAutoSaveIntervalMinutes > 0)
                {
                    _localAutoSaveCounterSeconds += deltaSeconds;
                    if (_localAutoSaveCounterSeconds >= State.LocalAutoSaveIntervalMinutes * 60)
                    {
                        _localAutoSaveCounterSeconds = 0;
                        _ = SaveAsync();
                    }
                }

                // Cloud Auto save (default 5 minutes or user configured)
                if (State.CloudAutoSaveIntervalMinutes > 0 && _cloudSaveService.IsConnected)
                {
                    _cloudAutoSaveCounterSeconds += deltaSeconds;
                    if (_cloudAutoSaveCounterSeconds >= State.CloudAutoSaveIntervalMinutes * 60)
                    {
                        _cloudAutoSaveCounterSeconds = 0;
                        _ = AutoSyncCloudAsync();
                    }
                }

                OnStateChanged?.Invoke();
                _ = _notificationService.NotifyAffordabilityChangedAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose
        }
    }

    private async Task AutoSyncCloudAsync()
    {
        try
        {
            if (_cloudSaveService.IsConnected)
            {
                var res = await _cloudSaveService.SyncAsync(State);
                if (res.Status == CloudSyncStatus.LocalUploadedToCloud)
                {
                    OnNotification?.Invoke("Cloud auto-save completed.");
                }
            }
        }
        catch { }
    }

    public void Tick(double deltaSeconds)
    {
        // 1. Tick Spells
        foreach (var spell in State.Spells)
        {
            spell.Tick(deltaSeconds);
        }

        // 2. Generate Passive Mana from Wizards
        double mps = State.CurrentMps;
        if (mps > 0)
        {
            double gain = mps * deltaSeconds;
            State.Mana += gain;
            State.LifetimeMana += gain;
        }

        State.LastTickTimeUtc = DateTime.UtcNow;
    }

    public void ClickFocusOrb()
    {
        double gain = State.ClickManaGain;
        State.Mana += gain;
        State.LifetimeMana += gain;
        State.TotalClicks++;

        OnManaClicked?.Invoke(gain);
        OnStateChanged?.Invoke();
        _ = _notificationService.NotifyAffordabilityChangedAsync();
    }

    public bool BuyWizard(string unitId, int quantity = 1)
    {
        var unit = State.Wizards.FirstOrDefault(w => w.Id == unitId);
        if (unit == null) return false;

        double cost = unit.GetCostForNext(quantity);
        if (State.Mana >= cost)
        {
            State.Mana -= cost;
            unit.Count += quantity;
            OnStateChanged?.Invoke();
            _ = _notificationService.NotifyAffordabilityChangedAsync();
            return true;
        }
        return false;
    }

    public bool BuyMaxWizard(string unitId)
    {
        var unit = State.Wizards.FirstOrDefault(w => w.Id == unitId);
        if (unit == null) return false;

        int max = unit.GetMaxAffordable(State.Mana);
        if (max <= 0) return false;

        return BuyWizard(unitId, max);
    }

    public bool BuyUpgrade(string upgradeId)
    {
        var upg = State.Upgrades.FirstOrDefault(u => u.Id == upgradeId);
        if (upg == null || upg.IsPurchased) return false;

        if (upg.CanAfford(State.Mana, State.ArcaneEssence, State.AstralShards))
        {
            State.Mana -= upg.CostMana;
            State.ArcaneEssence -= upg.CostEssence;
            State.AstralShards -= upg.CostAstralShards;
            upg.IsPurchased = true;

            OnNotification?.Invoke($"Researched: {upg.Name}!");
            OnStateChanged?.Invoke();
            _ = _notificationService.NotifyAffordabilityChangedAsync();
            return true;
        }
        return false;
    }

    public bool CastSpell(string spellId)
    {
        var spell = State.Spells.FirstOrDefault(s => s.Id == spellId);
        if (spell == null || !spell.CanCast(State.Mana, State.ArcaneEssence)) return false;

        State.Mana -= spell.ManaCost;
        State.ArcaneEssence -= spell.EssenceCost;

        spell.CurrentCooldownRemaining = spell.CooldownSeconds;
        spell.CurrentDurationRemaining = spell.DurationSeconds;

        switch (spell.EffectType)
        {
            case SpellEffectType.ArcaneSurge:
                OnNotification?.Invoke("Arcane Surge activated! 5x Click & MPS boost!");
                break;

            case SpellEffectType.TimeWarp:
                double instantGain = State.CurrentMps * spell.PowerMultiplier;
                State.Mana += instantGain;
                State.LifetimeMana += instantGain;
                OnNotification?.Invoke($"Temporal Warp collapsed! Gained {FormatNumber(instantGain)} Mana!");
                break;

            case SpellEffectType.Transmutation:
                double manaToConvert = State.Mana * spell.PowerMultiplier;
                // Minimum 100 mana converts to 1 essence
                double essenceEarned = Math.Max(1.0, Math.Floor(manaToConvert / 100.0));
                State.Mana -= manaToConvert;
                State.ArcaneEssence += essenceEarned;
                OnNotification?.Invoke($"Transmuted {FormatNumber(manaToConvert)} Mana into {essenceEarned:N0} Arcane Essence!");
                break;
        }

        OnStateChanged?.Invoke();
        _ = _notificationService.NotifyAffordabilityChangedAsync();
        return true;
    }

    public void Ascend()
    {
        double shards = State.CalculateAscensionReward();
        if (shards > 0)
        {
            State.PerformAscension();
            _ = SaveAsync();
            OnNotification?.Invoke($"Ascension Complete! Gained {shards:N0} Astral Shards!");
            OnStateChanged?.Invoke();
            _ = _notificationService.NotifyAffordabilityChangedAsync();
        }
    }

    public async Task SaveAsync()
    {
        await _saveStorage.SaveStateAsync(State);
    }

    public async Task ResetGameAsync()
    {
        await _saveStorage.ClearStateAsync();
        State = GameState.CreateDefault();
        OnNotification?.Invoke("Game has been reset.");
        OnStateChanged?.Invoke();
    }

    public void SetBuyQuantity(int quantity, bool isMax)
    {
        State.SelectedBuyQuantity = quantity;
        State.IsBuyMaxSelected = isMax;
        OnStateChanged?.Invoke();
        _ = _notificationService.NotifyAffordabilityChangedAsync();
    }

    public void UpdateAutoSaveSettings(int localMinutes, int cloudMinutes)
    {
        State.LocalAutoSaveIntervalMinutes = localMinutes;
        State.CloudAutoSaveIntervalMinutes = cloudMinutes;
        _localAutoSaveCounterSeconds = 0;
        _cloudAutoSaveCounterSeconds = 0;
        _ = SaveAsync();
        OnNotification?.Invoke("Auto-save configuration updated.");
        OnStateChanged?.Invoke();
    }

    public string ExportSaveString()
    {
        return SavePayload.SerializeToExportString(State);
    }

    public async Task<(bool Success, string Message)> ImportSaveStringAsync(string code)
    {
        var (success, importedState, error) = SavePayload.DeserializeFromExportString(code);
        if (!success || importedState == null)
        {
            return (false, error);
        }

        State = importedState;
        await SaveAsync();
        OnNotification?.Invoke("Save code successfully restored!");
        OnStateChanged?.Invoke();
        return (true, "Save restored successfully.");
    }

    public async Task ApplyCloudStateAsync(GameState cloudState)
    {
        State = cloudState;
        await SaveAsync();
        OnNotification?.Invoke("Loaded cloud save state!");
        OnStateChanged?.Invoke();
    }

    public static string FormatNumber(double num)
    {
        if (num < 1000) return num.ToString("0.#");
        if (num < 1_000_000) return (num / 1_000.0).ToString("0.##") + " K";
        if (num < 1_000_000_000) return (num / 1_000_000.0).ToString("0.##") + " M";
        if (num < 1_000_000_000_000) return (num / 1_000_000_000.0).ToString("0.##") + " B";
        if (num < 1_000_000_000_000_000) return (num / 1_000_000_000_000.0).ToString("0.##") + " T";
        if (num < 1_000_000_000_000_000_000) return (num / 1_000_000_000_000_000.0).ToString("0.##") + " Qa";
        return num.ToString("E2");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _tickTimer.Dispose();
        _cts.Dispose();
    }
}
