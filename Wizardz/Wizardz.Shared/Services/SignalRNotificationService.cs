using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Wizardz.Shared.Services;

public class SignalRNotificationService : IGameNotificationService, IAsyncDisposable
{
    private readonly NavigationManager? _navigationManager;
    private HubConnection? _hubConnection;
    private bool _isInitialized = false;

    public event Action? OnAffordabilityChanged;
    public event Action<string>? OnBroadcastReceived;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRNotificationService(NavigationManager? navigationManager = null)
    {
        _navigationManager = navigationManager;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        if (_navigationManager != null)
        {
            try
            {
                string baseUri = _navigationManager.BaseUri;
                // Only connect to SignalR hub if we have an HTTP/HTTPS endpoint
                if (baseUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    baseUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var hubUrl = _navigationManager.ToAbsoluteUri("/hubs/game");

                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl(hubUrl)
                        .WithAutomaticReconnect()
                        .Build();

                    _hubConnection.On<string>("AffordabilityUpdated", _ =>
                    {
                        OnAffordabilityChanged?.Invoke();
                    });

                    _hubConnection.On<string>("StateUpdated", message =>
                    {
                        OnBroadcastReceived?.Invoke(message);
                    });

                    await _hubConnection.StartAsync();
                }
            }
            catch
            {
                // Silently fallback to local in-process notification dispatch when hub is unavailable
            }
        }
    }

    public async Task NotifyAffordabilityChangedAsync()
    {
        // 1. Immediately invoke local subscribers for instant UI update
        OnAffordabilityChanged?.Invoke();

        // 2. Broadcast via SignalR if connected
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("NotifyAffordability", "shop");
            }
            catch
            {
                // Ignore network dispatch errors
            }
        }
    }

    public async Task BroadcastStateAsync(string message)
    {
        OnBroadcastReceived?.Invoke(message);

        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("BroadcastStateChange", message);
            }
            catch
            {
                // Ignore network dispatch errors
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.DisposeAsync();
            }
            catch { }
        }
    }
}
