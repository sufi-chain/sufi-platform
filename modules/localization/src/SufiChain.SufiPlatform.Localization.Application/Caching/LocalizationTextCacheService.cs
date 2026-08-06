using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Caching;
using SufiChain.SufiPlatform.Localization.Repositories;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Localization.Caching;

/// <summary>
/// Provides cached access to localization texts from the database.
/// Used by both <see cref="ExternalStore.DatabaseLocalizationResourceContributor"/> and
/// <see cref="ExternalStore.ConfiguredResourceLocalizationContributor"/> to avoid
/// hitting the database on every localization lookup.
/// </summary>
public class LocalizationTextCacheService : ISingletonDependency
{
    private readonly IDistributedCache<LocalizationTextCacheItem> _cache;
    private readonly IServiceProvider _serviceProvider;

    private static readonly TimeSpan CacheAbsoluteExpiration = TimeSpan.FromMinutes(5);

    public LocalizationTextCacheService(
        IDistributedCache<LocalizationTextCacheItem> cache,
        IServiceProvider serviceProvider)
    {
        _cache = cache;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets a single text value for the given resource, culture, and key.
    /// Returns null if not found in the database.
    /// </summary>
    public string? GetOrNull(string resourceName, string cultureName, string key)
    {
        // Run on thread pool to avoid sync-over-async deadlock when called from localization pipeline (e.g. Blazor sync context)
        return System.Threading.Tasks.Task.Run(() => GetOrNullAsync(resourceName, cultureName, key)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a single text value for the given resource, culture, and key.
    /// Returns null if not found in the database.
    /// </summary>
    public async Task<string?> GetOrNullAsync(string resourceName, string cultureName, string key)
    {
        var cacheItem = await GetCacheItemAsync(resourceName, cultureName).ConfigureAwait(false);
        return cacheItem.Texts.GetValueOrDefault(key);
    }

    /// <summary>
    /// Gets all texts for the given resource and culture as a dictionary.
    /// </summary>
    public async Task<Dictionary<string, string>> GetAllAsync(string resourceName, string cultureName)
    {
        var cacheItem = await GetCacheItemAsync(resourceName, cultureName).ConfigureAwait(false);
        return cacheItem.Texts;
    }

    /// <summary>
    /// Gets all texts synchronously for the given resource and culture.
    /// </summary>
    public Dictionary<string, string> GetAll(string resourceName, string cultureName)
    {
        // Run on thread pool to avoid sync-over-async deadlock when called from localization pipeline
        return System.Threading.Tasks.Task.Run(() => GetAllAsync(resourceName, cultureName)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Invalidates the cache for a specific resource + culture combination.
    /// Call this after create/update/delete operations.
    /// </summary>
    public async Task InvalidateAsync(string resourceName, string cultureName)
    {
        var cacheKey = LocalizationTextCacheItem.CalculateCacheKey(resourceName, cultureName);
        await _cache.RemoveAsync(cacheKey).ConfigureAwait(false);
    }

    /// <summary>
    /// Invalidates all cached cultures for a given resource.
    /// </summary>
    public async Task InvalidateResourceAsync(string resourceName)
    {
        // Since we can't enumerate cache keys with IDistributedCache,
        // we need to know the culture names to invalidate.
        // Query the DB for cultures used by this resource.
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILocalizationTextRepository>();
        var cultures = await repository.GetCultureNamesAsync(resourceName).ConfigureAwait(false);

        foreach (var culture in cultures)
        {
            await InvalidateAsync(resourceName, culture).ConfigureAwait(false);
        }
    }

    private async Task<LocalizationTextCacheItem> GetCacheItemAsync(string resourceName, string cultureName)
    {
        var cacheKey = LocalizationTextCacheItem.CalculateCacheKey(resourceName, cultureName);

        var cacheItem = await _cache.GetOrAddAsync(
            cacheKey,
            async () => await LoadFromDatabaseAsync(resourceName, cultureName).ConfigureAwait(false),
            () => new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheAbsoluteExpiration
            }).ConfigureAwait(false);

        return cacheItem ?? new LocalizationTextCacheItem();
    }

    private async Task<LocalizationTextCacheItem> LoadFromDatabaseAsync(string resourceName, string cultureName)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILocalizationTextRepository>();

        var cacheItem = new LocalizationTextCacheItem();
        foreach (var candidateCulture in GetCultureCandidates(cultureName).Reverse())
        {
            var texts = await repository.GetListAsync(resourceName, candidateCulture).ConfigureAwait(false);
            foreach (var text in texts)
            {
                cacheItem.Texts[text.Key] = text.Value;
            }
        }

        return cacheItem;
    }

    private static IEnumerable<string> GetCultureCandidates(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = cultureName.Trim();

        while (!string.IsNullOrWhiteSpace(current))
        {
            if (seen.Add(current))
            {
                yield return current;
            }

            var normalized = SeedCultureHelper.NormalizeCulture(current);
            if (!string.IsNullOrWhiteSpace(normalized) && seen.Add(normalized))
            {
                yield return normalized;
            }

            try
            {
                var parent = CultureInfo.GetCultureInfo(current).Parent?.Name;
                if (string.IsNullOrWhiteSpace(parent) || string.Equals(parent, current, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                current = parent;
            }
            catch (CultureNotFoundException)
            {
                break;
            }
        }
    }
}
