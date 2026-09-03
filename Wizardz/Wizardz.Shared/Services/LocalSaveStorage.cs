using System.Text.Json;
using Microsoft.JSInterop;
using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public class LocalSaveStorage : ISaveStorage
{
    private readonly IJSRuntime? _jsRuntime;
    private readonly string _saveFilePath;
    private const string LocalStorageKey = "wizardz_save_state_v1";

    public LocalSaveStorage(IJSRuntime? jsRuntime = null)
    {
        _jsRuntime = jsRuntime;

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string folder = Path.Combine(appData, "Wizardz");
        try
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
        catch
        {
            // Fallback for sandboxed or browser environments where directory creation might fail
            folder = appData;
        }
        _saveFilePath = Path.Combine(folder, "wizardz_save.json");
    }

    public async Task SaveStateAsync(GameState state)
    {
        state.LastSaveTimeUtc = DateTime.UtcNow;
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(state, jsonOptions);

        // 1. Try file system storage (Primary for MAUI on Windows / Android / iOS)
        try
        {
            string? dir = Path.GetDirectoryName(_saveFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            await File.WriteAllTextAsync(_saveFilePath, json);
        }
        catch
        {
            // Ignored in browser WASM sandbox where direct file writing might be disallowed
        }

        // 2. Try browser localStorage if JSRuntime is active (Primary for Web WASM)
        if (_jsRuntime != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
            }
            catch
            {
                // JSRuntime might not be available during prerender or if unsupported
            }
        }
    }

    public async Task<GameState?> LoadStateAsync()
    {
        string? json = null;

        // 1. Try browser localStorage first if available
        if (_jsRuntime != null)
        {
            try
            {
                json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LocalStorageKey);
            }
            catch
            {
                // Fallback to file system
            }
        }

        // 2. Try file system if localStorage was empty or unavailable
        if (string.IsNullOrWhiteSpace(json) && File.Exists(_saveFilePath))
        {
            try
            {
                json = await File.ReadAllTextAsync(_saveFilePath);
            }
            catch
            {
                // File read error
            }
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var state = JsonSerializer.Deserialize<GameState>(json);
            return state;
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearStateAsync()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
        }
        catch { }

        if (_jsRuntime != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", LocalStorageKey);
            }
            catch { }
        }
    }
}
