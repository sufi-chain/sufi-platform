namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Cache key format for menu trees. Public keys omit culture on purpose.
/// The public tree is stored unresolved and translated after it is read, so one
/// <c>pt:</c> removal covers every language.
/// </summary>
public static class MenuTreeCacheKeys
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

    public static IReadOnlyList<string> CreatePublicInvalidationKeys(
        string contextType,
        Guid? contextId,
        string menuName,
        string? slug)
    {
        var keys = new List<string>
        {
            CreatePublicTreeCacheKey(contextType, contextId, menuName)
        };

        if (!string.IsNullOrWhiteSpace(slug))
        {
            keys.Add(CreatePublicItemCacheKey(contextType, contextId, menuName, slug));
        }

        return keys;
    }
}
