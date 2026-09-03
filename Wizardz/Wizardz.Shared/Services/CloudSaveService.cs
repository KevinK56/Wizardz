using System.Text.Json;
using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public class CloudSaveService : ICloudSaveService
{
    private readonly string _mockCloudPath;
    private CloudProviderType _activeProvider = CloudProviderType.GoogleDrive;
    private bool _isConnected = false;
    private string _accountEmail = string.Empty;
    private DateTime? _lastCloudSyncUtc = null;

    public CloudProviderType ActiveProvider => _activeProvider;
    public bool IsConnected => _isConnected;
    public string ConnectedAccountEmail => _accountEmail;
    public DateTime? LastCloudSyncUtc => _lastCloudSyncUtc;

    public CloudSaveService()
    {
        string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wizardz", "CloudStorageMock");
        try
        {
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }
        }
        catch { }
        _mockCloudPath = Path.Combine(baseDir, "remote_cloud_save.json");
    }

    public Task<bool> ConnectCloudAccountAsync(CloudProviderType provider, string accountIdentifier)
    {
        _activeProvider = provider;
        _accountEmail = string.IsNullOrWhiteSpace(accountIdentifier) ? "wizard.archmage@arcane-cloud.net" : accountIdentifier;
        _isConnected = true;
        return Task.FromResult(true);
    }

    public Task DisconnectCloudAccountAsync()
    {
        _isConnected = false;
        _accountEmail = string.Empty;
        return Task.CompletedTask;
    }

    public async Task<CloudSyncResult> SyncAsync(GameState localState)
    {
        if (!_isConnected)
        {
            return new CloudSyncResult
            {
                Status = CloudSyncStatus.NotAuthenticated,
                Message = "No Cloud Account is connected. Connect Google Drive or Apple iCloud to sync."
            };
        }

        try
        {
            var cloudState = await ForceDownloadAsync();
            if (cloudState == null)
            {
                // No cloud save exists yet, upload local
                await ForceUploadAsync(localState);
                return new CloudSyncResult
                {
                    Status = CloudSyncStatus.LocalUploadedToCloud,
                    Message = "First cloud backup created successfully!"
                };
            }

            // Compare timestamps and progress
            TimeSpan timeDiff = cloudState.LastSaveTimeUtc - localState.LastSaveTimeUtc;
            
            // If cloud is notably newer (by > 30 seconds) and has greater or equal lifetime mana, flag conflict
            if (timeDiff.TotalSeconds > 30 && cloudState.LifetimeMana > localState.LifetimeMana)
            {
                return new CloudSyncResult
                {
                    Status = CloudSyncStatus.CloudNewerConflict,
                    Message = $"A newer save was found on your {_activeProvider} account ({cloudState.LastSaveTimeUtc.ToLocalTime():g}).",
                    CloudState = cloudState
                };
            }

            // Otherwise, local is equal or newer, so upload local
            await ForceUploadAsync(localState);
            _lastCloudSyncUtc = DateTime.UtcNow;

            return new CloudSyncResult
            {
                Status = CloudSyncStatus.LocalUploadedToCloud,
                Message = $"Cloud save synchronized with {_activeProvider}."
            };
        }
        catch (Exception ex)
        {
            return new CloudSyncResult
            {
                Status = CloudSyncStatus.Error,
                Message = $"Sync failed: {ex.Message}"
            };
        }
    }

    public async Task<bool> ForceUploadAsync(GameState localState)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string payload = JsonSerializer.Serialize(localState, jsonOptions);
            string? dir = Path.GetDirectoryName(_mockCloudPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            await File.WriteAllTextAsync(_mockCloudPath, payload);
            _lastCloudSyncUtc = DateTime.UtcNow;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<GameState?> ForceDownloadAsync()
    {
        try
        {
            if (!File.Exists(_mockCloudPath))
            {
                return null;
            }

            string json = await File.ReadAllTextAsync(_mockCloudPath);
            var state = JsonSerializer.Deserialize<GameState>(json);
            return state;
        }
        catch
        {
            return null;
        }
    }
}
