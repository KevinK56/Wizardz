using Wizardz.Shared.Models;
using Wizardz.Shared.Services;
using Xunit;

namespace Wizardz.Tests;

public class MockSaveStorage : ISaveStorage
{
    public GameState? SavedState { get; set; }
    public Task SaveStateAsync(GameState state)
    {
        SavedState = state;
        return Task.CompletedTask;
    }

    public Task<GameState?> LoadStateAsync()
    {
        return Task.FromResult(SavedState);
    }

    public Task ClearStateAsync()
    {
        SavedState = null;
        return Task.CompletedTask;
    }
}

public class MockNotificationService : IGameNotificationService
{
    public event Action? OnAffordabilityChanged;
    public event Action<string>? OnBroadcastReceived;
    public bool IsConnected => true;
    public int AffordabilityNotifyCount { get; private set; } = 0;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task NotifyAffordabilityChangedAsync()
    {
        AffordabilityNotifyCount++;
        OnAffordabilityChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task BroadcastStateAsync(string message)
    {
        OnBroadcastReceived?.Invoke(message);
        return Task.CompletedTask;
    }
}

public class GameEngineTests
{
    [Fact]
    public void TestInitialGameState_HasExpectedDefaults()
    {
        var storage = new MockSaveStorage();
        var cloud = new CloudSaveService();
        var notifier = new MockNotificationService();
        using var engine = new GameEngine(storage, cloud, notifier);

        Assert.Equal(0, engine.State.Mana);
        Assert.Equal(0, engine.State.CurrentMps);
        Assert.NotEmpty(engine.State.Wizards);
        Assert.NotEmpty(engine.State.Spells);
        Assert.NotEmpty(engine.State.Upgrades);
    }

    [Fact]
    public void TestClickOrb_GeneratesManaAndTriggersAffordabilityNotification()
    {
        var storage = new MockSaveStorage();
        var cloud = new CloudSaveService();
        var notifier = new MockNotificationService();
        using var engine = new GameEngine(storage, cloud, notifier);

        bool notified = false;
        notifier.OnAffordabilityChanged += () => notified = true;

        double initialMana = engine.State.Mana;
        engine.ClickFocusOrb();

        Assert.True(engine.State.Mana > initialMana);
        Assert.Equal(1, engine.State.TotalClicks);
        Assert.True(notified);
        Assert.True(notifier.AffordabilityNotifyCount > 0);
    }

    [Fact]
    public void TestBuyWizard_DeductsManaAndIncreasesCountAndMps()
    {
        var storage = new MockSaveStorage();
        var cloud = new CloudSaveService();
        var notifier = new MockNotificationService();
        using var engine = new GameEngine(storage, cloud, notifier);

        // Give player enough mana to buy 1 novice
        engine.State.Mana = 100;
        var novice = engine.State.Wizards.First(w => w.Id == "novice");
        double cost = novice.GetCostForNext(1);

        bool bought = engine.BuyWizard("novice", 1);

        Assert.True(bought);
        Assert.Equal(1, novice.Count);
        Assert.Equal(100 - cost, engine.State.Mana);
        Assert.Equal(1.0, engine.State.CurrentMps);
    }

    [Fact]
    public void TestGeometricSeriesCostCalculation()
    {
        var unit = new WizardUnit
        {
            BaseCost = 10,
            CostMultiplier = 1.15,
            Count = 0
        };

        // Single cost
        Assert.Equal(10, unit.GetCostForNext(1));

        // 5 units cost
        double bulkCost = unit.GetCostForNext(5);
        double manualSum = 0;
        for (int i = 0; i < 5; i++)
        {
            manualSum += Math.Floor(10 * Math.Pow(1.15, i));
        }

        // Within reasonable floor approximation
        Assert.True(Math.Abs(bulkCost - manualSum) <= 5);
    }

    [Fact]
    public void TestSaveAndRestorePayload_RoundtripSucceeds()
    {
        var state = GameState.CreateDefault();
        state.Mana = 42_1337;
        state.ArcaneEssence = 99;
        state.LifetimeMana = 1_000_000;
        state.Wizards.First().Count = 5;

        string exportString = SavePayload.SerializeToExportString(state);
        Assert.False(string.IsNullOrWhiteSpace(exportString));

        var (success, restoredState, error) = SavePayload.DeserializeFromExportString(exportString);

        Assert.True(success, error);
        Assert.NotNull(restoredState);
        Assert.Equal(42_1337, restoredState!.Mana);
        Assert.Equal(99, restoredState.ArcaneEssence);
        Assert.Equal(5, restoredState.Wizards.First().Count);
    }

    [Fact]
    public void TestCastSpell_ArcaneSurgeAppliesMultiplier()
    {
        var storage = new MockSaveStorage();
        var cloud = new CloudSaveService();
        var notifier = new MockNotificationService();
        using var engine = new GameEngine(storage, cloud, notifier);

        engine.State.Mana = 500;
        var surge = engine.State.Spells.First(s => s.Id == "arcane_surge");

        double normalClick = engine.State.ClickManaGain;
        bool cast = engine.CastSpell("arcane_surge");

        Assert.True(cast);
        Assert.True(surge.IsActive);
        Assert.True(engine.State.ClickManaGain > normalClick);
    }

    [Fact]
    public void TestAscension_CalculatesRewardAndResetsCorrectly()
    {
        var state = GameState.CreateDefault();
        state.Mana = 500_000;
        state.LifetimeMana = 4_000_000; // 4 Million -> 150 * sqrt(4) = 300 Astral Shards
        state.Wizards.First().Count = 10;

        double expectedReward = state.CalculateAscensionReward();
        Assert.Equal(300, expectedReward);

        state.PerformAscension();

        Assert.Equal(0, state.Mana);
        Assert.Equal(0, state.Wizards.First().Count);
        Assert.Equal(300, state.AstralShards);
        Assert.Equal(1, state.AscensionCount);
    }

    [Fact]
    public void TestBuyQuantityRememberedAndPersisted()
    {
        var storage = new MockSaveStorage();
        var cloud = new CloudSaveService();
        var notifier = new MockNotificationService();
        using var engine = new GameEngine(storage, cloud, notifier);

        Assert.Equal(1, engine.State.SelectedBuyQuantity);
        Assert.False(engine.State.IsBuyMaxSelected);

        engine.SetBuyQuantity(10, false);
        Assert.Equal(10, engine.State.SelectedBuyQuantity);
        Assert.False(engine.State.IsBuyMaxSelected);

        engine.SetBuyQuantity(10, true);
        Assert.Equal(10, engine.State.SelectedBuyQuantity);
        Assert.True(engine.State.IsBuyMaxSelected);
    }

    [Fact]
    public void TestUpdateAutoSaveSettings()
    {
        var storage = new MockSaveStorage();
        var cloud = new CloudSaveService();
        var notifier = new MockNotificationService();
        using var engine = new GameEngine(storage, cloud, notifier);

        // Verify defaults
        Assert.Equal(2, engine.State.LocalAutoSaveIntervalMinutes);
        Assert.Equal(5, engine.State.CloudAutoSaveIntervalMinutes);

        // Update settings
        engine.UpdateAutoSaveSettings(5, 10);
        Assert.Equal(5, engine.State.LocalAutoSaveIntervalMinutes);
        Assert.Equal(10, engine.State.CloudAutoSaveIntervalMinutes);
    }
}
