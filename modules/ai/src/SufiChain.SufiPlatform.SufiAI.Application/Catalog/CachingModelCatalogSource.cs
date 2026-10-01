using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public class CachingModelCatalogSource : IModelCatalogSource, ITransientDependency
{
    private readonly IEnumerable<IModelCatalogProvider> _providers;
    private readonly IDistributedCache<ModelCatalogCacheItem> _cache;
    private readonly DistributedRefreshGate _refresh;
    private readonly AIOptions _options;
    private readonly ILogger<CachingModelCatalogSource> _logger;

    public CachingModelCatalogSource(
        IEnumerable<IModelCatalogProvider> providers,
        IDistributedCache<ModelCatalogCacheItem> cache,
        DistributedRefreshGate refresh,
        IOptions<AIOptions> options,
        ILogger<CachingModelCatalogSource> logger)
    {
        _providers = providers;
        _cache = cache;
        _refresh = refresh;
        _options = options.Value;
        _logger = logger;
    }

    public Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        return GetModelsAsync(catalogName: null, cancellationToken);
    }

    public async Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(
        string? catalogName,
        CancellationToken cancellationToken = default)
    {
        var provider = SelectProvider(catalogName);
        if (provider == null)
        {
            return null;
        }

        var key = CacheKey(provider.Name);
        try
        {
            var item = await _refresh.ReadOrRefreshAsync(
                _cache,
                key,
                "SufiAI:ModelCatalog:" + provider.Name,
                _options.ProviderCatalogCacheSeconds,
                () => LoadAsync(provider, key, cancellationToken),
                cached => cached.Unavailable,
                cancellationToken);
            if (item == null || item.Unavailable)
            {
                return item?.Models.Count > 0 ? item.Models : null;
            }

            return item.Models;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Model catalog cache fill failed.");
            return null;
        }
    }

    public Task<ModelCatalogEntry?> FindAsync(string modelId, CancellationToken cancellationToken = default)
    {
        return FindAsync(modelId, catalogName: null, cancellationToken);
    }

    public async Task<ModelCatalogEntry?> FindAsync(
        string modelId,
        string? catalogName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return null;
        }

        var models = await GetModelsAsync(catalogName, cancellationToken);
        if (models == null)
        {
            return null;
        }

        var match = ModelCatalogMatcher.Match(models, modelId);
        if (match != null || !ModelCatalogMatcher.NeedsAuthorSlugLookup(modelId))
        {
            return match;
        }

        var provider = SelectProvider(catalogName);
        if (provider == null)
        {
            return null;
        }

        var key = CacheKey(provider.Name);
        var cached = await _cache.GetAsync(key, token: cancellationToken);
        var lookupId = ModelCatalogMatcher.StripNitro(modelId.Trim());
        if (cached?.UnknownIds.Any(id => string.Equals(id, lookupId, StringComparison.OrdinalIgnoreCase)) == true)
        {
            return null;
        }

        await using var handle = await _refresh.TryAcquireAsync(
            "SufiAI:ModelCatalog:" + provider.Name,
            cancellationToken);
        if (handle == null)
        {
            cached = await _cache.GetAsync(key, token: cancellationToken);
            return cached == null ? null : ModelCatalogMatcher.Match(cached.Models, modelId);
        }

        var lookup = await provider.TryFindAsync(lookupId, cancellationToken);
        if (lookup.Status == ModelCatalogLookupStatus.Unavailable || cached == null)
        {
            return lookup.Entry;
        }

        if (lookup.Status == ModelCatalogLookupStatus.Unknown)
        {
            cached.UnknownIds.Add(lookupId);
            await _cache.SetAsync(key, cached, token: cancellationToken);
            return null;
        }

        if (lookup.Entry != null && cached.Models.All(entry => !string.Equals(entry.Id, lookup.Entry.Id, StringComparison.OrdinalIgnoreCase)))
        {
            cached.Models.Add(lookup.Entry);
            await _cache.SetAsync(key, cached, token: cancellationToken);
        }

        return lookup.Entry;
    }

    private async Task<ModelCatalogCacheItem?> LoadAsync(
        IModelCatalogProvider provider,
        string key,
        CancellationToken cancellationToken)
    {
        var existing = await _cache.GetAsync(key, token: cancellationToken);
        var models = await provider.TryListAsync(cancellationToken);
        if (models == null)
        {
            if (existing is { Models.Count: > 0 })
            {
                return existing;
            }

            return new ModelCatalogCacheItem
            {
                ProviderName = provider.Name,
                Unavailable = true
            };
        }

        return new ModelCatalogCacheItem
        {
            ProviderName = provider.Name,
            Models = models.ToList()
        };
    }

    private IModelCatalogProvider? SelectProvider(string? catalogName)
    {
        var name = string.IsNullOrWhiteSpace(catalogName) ? null : catalogName.Trim();
        if (name == null)
        {
            return null;
        }

        return _providers.FirstOrDefault(provider => string.Equals(provider.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string CacheKey(string providerName)
    {
        return "ai:model-catalog:v2:" + providerName.Trim().ToLowerInvariant();
    }
}
