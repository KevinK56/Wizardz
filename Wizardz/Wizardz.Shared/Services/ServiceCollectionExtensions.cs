using Microsoft.Extensions.DependencyInjection;

namespace Wizardz.Shared.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWizardzGame(this IServiceCollection services)
    {
        services.AddScoped<ISaveStorage, LocalSaveStorage>();
        services.AddScoped<ICloudSaveService, CloudSaveService>();
        services.AddScoped<GameEngine>();
        return services;
    }
}
