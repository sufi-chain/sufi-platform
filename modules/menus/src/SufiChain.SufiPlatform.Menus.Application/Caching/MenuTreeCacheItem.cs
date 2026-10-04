using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Menus.Caching;

/// <summary>
/// Caches built menu trees and resolved public items, keyed by the lookup input.
/// ABP prefixes the cache key with the current tenant automatically. Uses a distinct
/// <see cref="CacheNameAttribute"/> from <see cref="MenuCacheItem"/> so ABP keeps
/// separate unit-of-work cache buckets per item type (they store different generics).
/// </summary>
[CacheName("SufiMenuTrees")]
public class MenuTreeCacheItem
{
    public const string TreePrefix = MenuTreeCacheKeys.TreePrefix;
    public const string PublicTreePrefix = MenuTreeCacheKeys.PublicTreePrefix;
    public const string PublicItemPrefix = MenuTreeCacheKeys.PublicItemPrefix;

    public static string CreateTreeCacheKey(Guid menuId, bool publicOnly) =>
        MenuTreeCacheKeys.CreateTreeCacheKey(menuId, publicOnly);

    /// <summary>
    /// Public trees are cached before localization. The key has no culture.
    /// Callers translate a copy after the read, so removing this one key clears every language.
    /// </summary>
    public static string CreatePublicTreeCacheKey(string contextType, Guid? contextId, string menuName) =>
        MenuTreeCacheKeys.CreatePublicTreeCacheKey(contextType, contextId, menuName);

    public static string CreatePublicItemCacheKey(string contextType, Guid? contextId, string menuName, string slug) =>
        MenuTreeCacheKeys.CreatePublicItemCacheKey(contextType, contextId, menuName, slug);

    public List<Menus.MenuItemTreeDto> Tree { get; set; } = new();
    public Menus.MenuItemDto? Item { get; set; }
}
