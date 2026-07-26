using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Tags.Caching;

/// <summary>
/// Caches tag links and the tags resolved for an entity. ABP prefixes the cache
/// key with the current tenant automatically.
/// </summary>
[CacheName("SufiTagLinks")]
public class TagLinkCacheItem
{
    public const string EntityTagsPrefix = "e:";

    public static string CreateEntityTagsCacheKey(string entityType, Guid entityId) =>
        $"{EntityTagsPrefix}{entityType}:{entityId}";

    public List<Tags.TagDto> Tags { get; set; } = new();
}
