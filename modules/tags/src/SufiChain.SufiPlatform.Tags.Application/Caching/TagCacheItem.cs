using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Tags.Caching;

/// <summary>
/// Caches tag lookups. ABP prefixes the cache key with the current tenant
/// automatically, so keys omit the tenant id.
/// </summary>
[CacheName("SufiTags")]
public class TagCacheItem
{
    public const string ScopeListPrefix = "s:";

    public static string CreateScopeListCacheKey(string scope) => $"{ScopeListPrefix}{scope}";

    public List<Tags.TagDto> Tags { get; set; } = new();
}
