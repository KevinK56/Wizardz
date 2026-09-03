namespace Wizardz.Shared.Services;

public interface IGameNotificationService
{
    event Action? OnAffordabilityChanged;
    event Action<string>? OnBroadcastReceived;

    Task InitializeAsync();
    Task NotifyAffordabilityChangedAsync();
    Task BroadcastStateAsync(string message);

    bool IsConnected { get; }
}
