using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform;

/// <summary>
/// Host bootstrap helpers so MAUI / console hosts call Sufi APIs without importing Volo types at call sites.
/// </summary>
public static class SufiHostApplicationExtensions
{
    public static Task AddSufiApplicationAsync<TModule>(this IServiceCollection services)
        where TModule : IAbpModule
    {
        return services.AddApplicationAsync<TModule>();
    }

    public static Task InitializeSufiAsync(this IServiceProvider services)
    {
        return services
            .GetRequiredService<IAbpApplicationWithExternalServiceProvider>()
            .InitializeAsync(services);
    }
}
