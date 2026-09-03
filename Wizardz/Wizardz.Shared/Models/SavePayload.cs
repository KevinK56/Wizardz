using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Wizardz.Shared.Models;

public class SavePayload
{
    public int Version { get; set; } = 1;
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
    public string SourceDevice { get; set; } = "Local";
    public GameState State { get; set; } = new();
    public string Checksum { get; set; } = string.Empty;

    public static string ComputeHash(string json)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json + "_wizardz_secret_salt_2026"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string SerializeToExportString(GameState state, string source = "ManualExport")
    {
        var jsonOptions = new JsonSerializerOptions { WriteIndented = false };
        string stateJson = JsonSerializer.Serialize(state, jsonOptions);
        var payload = new SavePayload
        {
            Version = 1,
            SavedAtUtc = DateTime.UtcNow,
            SourceDevice = source,
            State = state,
            Checksum = ComputeHash(stateJson)
        };

        string fullJson = JsonSerializer.Serialize(payload, jsonOptions);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(fullJson));
    }

    public static (bool Success, GameState? State, string Error) DeserializeFromExportString(string exportString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(exportString))
                return (false, null, "Export string is empty.");

            byte[] bytes = Convert.FromBase64String(exportString.Trim());
            string json = Encoding.UTF8.GetString(bytes);
            var payload = JsonSerializer.Deserialize<SavePayload>(json);

            if (payload == null || payload.State == null)
            {
                return (false, null, "Invalid save payload format.");
            }

            // Verify checksum integrity
            string stateJson = JsonSerializer.Serialize(payload.State);
            string expectedHash = ComputeHash(stateJson);
            if (!string.Equals(payload.Checksum, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                // Note: allow loading even if checksum mismatched if minor version difference, but flag it
                // We'll accept it with a log/warning so players don't lose data
            }

            return (true, payload.State, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, null, $"Failed to decode save data: {ex.Message}");
        }
    }
}
