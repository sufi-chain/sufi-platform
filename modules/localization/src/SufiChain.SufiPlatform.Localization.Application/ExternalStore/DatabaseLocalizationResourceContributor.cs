using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Localization.Caching;
using SufiChain.SufiPlatform.Localization.Repositories;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Localization.ExternalStore;

/// <summary>
/// Provides translations from the database for a specific localization resource.
/// This contributor is dynamic, meaning it can be updated at runtime.
/// Uses <see cref="LocalizationTextCacheService"/> for cached access.
/// </summary>
public class DatabaseLocalizationResourceContributor : ILocalizationResourceContributor
{
    public bool IsDynamic => true;

    private readonly string _resourceName;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseLocalizationResourceContributor(
        string resourceName,
        IServiceProvider serviceProvider)
    {
        _resourceName = resourceName;
        _serviceProvider = serviceProvider;
    }

    public void Initialize(LocalizationResourceInitializationContext context)
    {
    }

    public LocalizedString? GetOrNull(string cultureName, string name)
    {
        try
        {
            var cacheService = _serviceProvider.GetService<LocalizationTextCacheService>();
            if (cacheService == null)
            {
                return null;
            }

            var value = cacheService.GetOrNull(_resourceName, cultureName, name);
            return value != null ? new LocalizedString(name, value) : null;
        }
        catch
        {
            return null;
        }
    }

    public void Fill(string cultureName, Dictionary<string, LocalizedString> dictionary)
    {
        // Run on thread pool to avoid sync-over-async deadlock when framework calls Fill from Blazor/ASP.NET sync context
        System.Threading.Tasks.Task.Run(() => FillAsync(cultureName, dictionary)).GetAwaiter().GetResult();
    }

    public async Task FillAsync(string cultureName, Dictionary<string, LocalizedString> dictionary)
    {
        try
        {
            var cacheService = _serviceProvider.GetService<LocalizationTextCacheService>();
            if (cacheService == null)
            {
                return;
            }

            var texts = await cacheService.GetAllAsync(_resourceName, cultureName).ConfigureAwait(false);

            foreach (var (key, value) in texts)
            {
                dictionary[key] = new LocalizedString(key, value);
            }
        }
        catch
        {
            // If cache service is not available, skip
        }
    }

    public async Task<IEnumerable<string>> GetSupportedCulturesAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ILocalizationTextRepository>();

            return await repository.GetCultureNamesAsync(_resourceName).ConfigureAwait(false);
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }
}
