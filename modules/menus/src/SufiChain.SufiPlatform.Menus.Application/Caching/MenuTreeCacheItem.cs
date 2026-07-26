using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Menus.Caching;

/// <summary>
/// Caches built menu trees and resolved public items, keyed by the lookup input.
/// ABP prefixes the cache key with the current tenant automatically.
/// </summary>
[CacheName("SufiMenus")]
public class MenuTreeCacheItem
{
    public const string TreePrefix = "t:";
    public const string PublicTreePrefix = "pt:";
    public const string PublicItemPrefix = "pi:";

    public static string CreateTreeCacheKey(Guid menuId, bool publicOnly) =>
        $"{TreePrefix}{menuId}:{publicOnly}";

    public static string CreatePublicTreeCacheKey(string contextType, Guid? contextId, string menuName) =>
        $"{PublicTreePrefix}{contextType}:{contextId ?? Guid.Empty}:{menuName}";

    public static string CreatePublicItemCacheKey(string contextType, Guid? contextId, string menuName, string slug) =>
        $"{PublicItemPrefix}{contextType}:{contextId ?? Guid.Empty}:{menuName}:{slug}";

    public List<Menus.MenuItemTreeDto> Tree { get; set; } = new();
    public Menus.MenuItemDto? Item { get; set; }
}
