using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Menus.Caching;

/// <summary>
/// Caches a single resolved menu aggregate, keyed by context + name. ABP
/// prefixes the cache key with the current tenant automatically.
/// </summary>
[CacheName("SufiMenus")]
public class MenuCacheItem
{
    public static string CreateCacheKey(string contextType, Guid? contextId, string name) =>
        $"m:{contextType}:{contextId ?? Guid.Empty}:{name}";

    public Menus.Menu? Menu { get; set; }
}
