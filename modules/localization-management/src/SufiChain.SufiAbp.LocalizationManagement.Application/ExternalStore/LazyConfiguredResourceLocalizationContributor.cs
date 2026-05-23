using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.LocalizationManagement.Caching;
using SufiChain.SufiAbp.LocalizationManagement.Repositories;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.LocalizationManagement.ExternalStore;

/// <summary>
/// A localization resource contributor that provides database overrides for
/// ABP-configured resources. Unlike <see cref="ConfiguredResourceLocalizationContributor"/>,
/// this version resolves its dependencies lazily from the <see cref="LocalizationResourceInitializationContext"/>
/// so it can be added during module configuration (before the service provider is built).
/// </summary>
public class LazyConfiguredResourceLocalizationContributor : ILocalizationResourceContributor
{
    public bool IsDynamic => true;

    private readonly string _resourceName;
    private IServiceProvider? _serviceProvider;

    public LazyConfiguredResourceLocalizationContributor(string resourceName)
    {
        _resourceName = resourceName;
    }

    public void Initialize(LocalizationResourceInitializationContext context)
    {
        _serviceProvider = context.ServiceProvider;
    }

    public LocalizedString? GetOrNull(string cultureName, string name)
    {
        if (_serviceProvider == null)
        {
            return null;
        }

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
        if (_serviceProvider == null)
        {
            return;
        }

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
            // If cache service is not available (e.g., during startup), skip
        }
    }

    public async Task<IEnumerable<string>> GetSupportedCulturesAsync()
    {
        if (_serviceProvider == null)
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetService<ILocalizationTextRepository>();

            if (repository == null)
            {
                return Enumerable.Empty<string>();
            }

            return await repository.GetCultureNamesAsync(_resourceName).ConfigureAwait(false);
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }
}
