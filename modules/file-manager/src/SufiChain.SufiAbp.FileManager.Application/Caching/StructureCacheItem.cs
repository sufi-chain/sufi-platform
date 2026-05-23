using Volo.Abp.Caching;

namespace SufiChain.SufiAbp.FileManager.Caching;

[CacheName("FileStructure")]
public class StructureCacheItem
{
    public const string CacheKey = "All";

    public Dictionary<string, StructureCacheEntry> StructuresByKey { get; set; } = new();
}
