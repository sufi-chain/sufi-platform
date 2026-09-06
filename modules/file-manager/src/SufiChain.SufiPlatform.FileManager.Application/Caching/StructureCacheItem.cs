using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.FileManager.Caching;

[CacheName("FileStructure")]
public class StructureCacheItem
{
    public const string CacheKeyPrefix = "All";

    public static string GetCacheKey(Guid? tenantId) =>
        $"{CacheKeyPrefix}:{tenantId?.ToString() ?? "host"}";

    public Dictionary<string, StructureCacheEntry> StructuresByKey { get; set; } = new();
}
