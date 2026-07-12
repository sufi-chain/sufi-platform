using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace SufiChain.SufiPlatform.FileManager.Caching;

public class StructureCacheService : IStructureCache, ITransientDependency
{
    private readonly IDistributedCache<StructureCacheItem> _cache;
    private readonly IFileStructureRepository _structureRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly ILogger<StructureCacheService> _logger;

    private static readonly Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public StructureCacheService(
        IDistributedCache<StructureCacheItem> cache,
        IFileStructureRepository structureRepository,
        IAsyncQueryableExecuter asyncExecuter,
        ILogger<StructureCacheService> logger)
    {
        _cache = cache;
        _structureRepository = structureRepository;
        _asyncExecuter = asyncExecuter;
        _logger = logger;
    }

    public async Task<StructureCacheEntry?> GetAsync(string? structureKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(structureKey))
            return null;

        var all = await GetAllAsync(cancellationToken);
        return all.TryGetValue(structureKey, out var entry) ? entry : null;
    }

    public async Task<HashSet<string>> GetPublicStructureKeysAsync(CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all
            .Where(x => x.Value.IsPublicAccess)
            .Select(x => x.Key)
            .ToHashSet();
    }

    public async Task<bool> IsPublicAccessAsync(string? structureKey, CancellationToken cancellationToken = default)
    {
        var entry = await GetAsync(structureKey, cancellationToken);
        return entry?.IsPublicAccess ?? false;
    }

    public async Task<IReadOnlyDictionary<string, StructureCacheEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var item = await _cache.GetOrAddAsync(
            StructureCacheItem.CacheKey,
            async () => await LoadFromDatabaseAsync(cancellationToken),
            () => CacheOptions);

        return item?.StructuresByKey ?? new Dictionary<string, StructureCacheEntry>();
    }

    private async Task<StructureCacheItem> LoadFromDatabaseAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading file structures from database into cache");

        var query = await _structureRepository.GetQueryableAsync();
        var structures = await _asyncExecuter.ToListAsync(query, cancellationToken);

        var dict = new Dictionary<string, StructureCacheEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in structures)
        {
            if (string.IsNullOrEmpty(s.Key))
            {
                _logger.LogWarning("Skipping file structure with null or empty Key (Id={StructureId})", s.Id);
                continue;
            }
            dict[s.Key] = MapToCacheEntry(s);
        }

        return new StructureCacheItem { StructuresByKey = dict };
    }

    private static StructureCacheEntry MapToCacheEntry(FileStructures.FileStructure s)
    {
        var entry = new StructureCacheEntry
        {
            Key = s.Key,
            IsPublicAccess = s.IsPublicAccess,
            BaseUrl = s.BaseUrl,
            MaxFileSize = s.MaxFileSize,
            AllowedExtensions = s.AllowedExtensions ?? "",
            AllowedMimeTypes = s.AllowedMimeTypes ?? "",
            GenerateThumbnail = s.GenerateThumbnail,
            ThumbnailWidth = s.ThumbnailWidth,
            ThumbnailHeight = s.ThumbnailHeight,
            EnableWebPConversion = s.EnableWebPConversion,
            WebPQuality = s.WebPQuality,
            AllowedFileTypes = s.AllowedFileTypes,
            MinImageWidth = s.MinImageWidth,
            MinImageHeight = s.MinImageHeight,
            MaxImageWidth = s.MaxImageWidth,
            MaxImageHeight = s.MaxImageHeight
        };

        if (s.ExtraProperties != null && s.ExtraProperties.Count > 0)
        {
            entry.ExtraProperties = new Dictionary<string, object>(s.ExtraProperties);
        }

        return entry;
    }
}
