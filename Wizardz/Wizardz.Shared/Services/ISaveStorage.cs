using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public interface ISaveStorage
{
    Task SaveStateAsync(GameState state);
    Task<GameState?> LoadStateAsync();
    Task ClearStateAsync();
}
