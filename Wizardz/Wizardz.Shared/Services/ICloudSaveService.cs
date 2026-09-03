using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public enum CloudProviderType
{
    SimulatedCloud,
    GooglePlayGames,
    GoogleDrive,
    AppleICloud,
    CustomServer
}

public enum CloudSyncStatus
{
    InSync,
    LocalUploadedToCloud,
    CloudNewerConflict,
    CloudDownloaded,
    NotAuthenticated,
    Error
}

public class CloudSyncResult
{
    public CloudSyncStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public GameState? CloudState { get; set; }
}

public interface ICloudSaveService
{
    CloudProviderType ActiveProvider { get; }
    bool IsConnected { get; }
    string ConnectedAccountEmail { get; }
    DateTime? LastCloudSyncUtc { get; }

    Task<bool> ConnectCloudAccountAsync(CloudProviderType provider, string accountIdentifier);
    Task DisconnectCloudAccountAsync();
    Task<CloudSyncResult> SyncAsync(GameState localState);
    Task<bool> ForceUploadAsync(GameState localState);
    Task<GameState?> ForceDownloadAsync();
}
