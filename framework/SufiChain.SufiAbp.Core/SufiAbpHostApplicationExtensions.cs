using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp;

/// <summary>
/// Host bootstrap helpers so MAUI / console hosts call SufiAbp APIs without importing Volo types at call sites.
/// </summary>
public static class SufiAbpHostApplicationExtensions
{
    public static Task AddSufiAbpApplicationAsync<TModule>(this IServiceCollection services)
        where TModule : IAbpModule
    {
        return services.AddApplicationAsync<TModule>();
    }

    public static Task InitializeSufiAbpAsync(this IServiceProvider services)
    {
        return services
            .GetRequiredService<IAbpApplicationWithExternalServiceProvider>()
            .InitializeAsync(services);
    }
}
