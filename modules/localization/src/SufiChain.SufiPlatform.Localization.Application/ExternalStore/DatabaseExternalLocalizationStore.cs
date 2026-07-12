using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Localization.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.Localization.External;

namespace SufiChain.SufiPlatform.Localization.ExternalStore;

/// <summary>
/// Database-backed implementation of IExternalLocalizationStore.
/// This allows localization resources to be discovered from the database
/// in addition to the configured resources in ABpLocalizationOptions.
/// </summary>
public class DatabaseExternalLocalizationStore : IExternalLocalizationStore, ISingletonDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, LocalizationResourceBase> _resourceCache = new();

    public DatabaseExternalLocalizationStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public LocalizationResourceBase? GetResourceOrNull(string resourceName)
    {
        return GetResourceOrNullAsync(resourceName).GetAwaiter().GetResult();
    }

    public async Task<LocalizationResourceBase?> GetResourceOrNullAsync(string resourceName)
    {
        if (_resourceCache.TryGetValue(resourceName, out var cached))
        {
            return cached;
        }

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILocalizationResourceRepository>();

        var dbResource = await repository.FindByNameAsync(resourceName);
        if (dbResource == null || !dbResource.IsEnabled)
        {
            return null;
        }

        var resource = new ExternalLocalizationResource(
            dbResource.ResourceName,
            dbResource.DefaultCulture,
            new DatabaseLocalizationResourceContributor(resourceName, _serviceProvider));

        foreach (var baseName in dbResource.BaseResourceNames)
        {
            resource.BaseResourceNames.Add(baseName);
        }

        _resourceCache.TryAdd(resourceName, resource);
        return resource;
    }

    public async Task<string[]> GetResourceNamesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILocalizationResourceRepository>();

        var names = await repository.GetResourceNamesAsync(enabledOnly: true);
        return names.ToArray();
    }

    public async Task<LocalizationResourceBase[]> GetResourcesAsync()
    {
        var names = await GetResourceNamesAsync();
        var resources = new LocalizationResourceBase[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            var resource = await GetResourceOrNullAsync(names[i]);
            if (resource != null)
            {
                resources[i] = resource;
            }
        }

        return resources.Where(r => r != null).ToArray()!;
    }

    /// <summary>
    /// Clears the cache for a specific resource (call when resource is updated)
    /// </summary>
    public void ClearCache(string resourceName)
    {
        _resourceCache.TryRemove(resourceName, out _);
    }

    /// <summary>
    /// Clears all cached resources
    /// </summary>
    public void ClearAllCache()
    {
        _resourceCache.Clear();
    }
}
