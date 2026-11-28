namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Maps an internal menu item onto the anonymous response and drops items that require a permission.
/// </summary>
public static class PublicMenuPresentation
{
    public static bool IsAnonymous(MenuItemDto? item) =>
        item != null && string.IsNullOrWhiteSpace(item.PermissionName);

    public static PublicMenuItemDto? ToItem(MenuItemDto? item, ISet<string>? granted = null)
    {
        if (item == null || !IsVisible(item, granted))
        {
            return null;
        }

        var copy = new PublicMenuItemDto();
        Copy(item, copy);
        return copy;
    }

    public static List<PublicMenuItemTreeDto> ToTree(IEnumerable<MenuItemTreeDto> items, ISet<string>? granted = null)
    {
        var result = new List<PublicMenuItemTreeDto>();
        foreach (var item in items)
        {
            if (!IsVisible(item, granted))
            {
                continue;
            }

            var copy = new PublicMenuItemTreeDto();
            Copy(item, copy);
            copy.Children = ToTree(item.Children, granted);
            result.Add(copy);
        }

        return result;
    }

    public static bool IsVisible(MenuItemDto? item, ISet<string>? granted)
    {
        if (item == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.PermissionName))
        {
            return true;
        }

        return granted != null && granted.Contains(item.PermissionName);
    }

    public static HashSet<string> PermissionNames(IEnumerable<MenuItemTreeDto> items)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        Collect(items, names);
        return names;
    }

    private static void Collect(IEnumerable<MenuItemTreeDto> items, HashSet<string> names)
    {
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.PermissionName))
            {
                names.Add(item.PermissionName);
            }

            Collect(item.Children, names);
        }
    }

    private static void Copy(MenuItemDto source, PublicMenuItemDto target)
    {
        target.Id = source.Id;
        target.MenuId = source.MenuId;
        target.ParentId = source.ParentId;
        target.Name = source.Name;
        target.DisplayName = source.DisplayName;
        target.Slug = source.Slug;
        target.Description = source.Description;
        target.DisplayOrder = source.DisplayOrder;
        target.Kind = source.Kind;
        target.DisplayType = source.DisplayType;
        target.Url = source.Url;
        target.CultureUrls = source.CultureUrls == null
            ? null
            : new Dictionary<string, string>(source.CultureUrls, StringComparer.OrdinalIgnoreCase);
        target.LinkTarget = source.LinkTarget;
        target.TargetType = source.TargetType;
        target.TargetId = source.TargetId;
        target.Icon = source.Icon;
        target.CssClass = source.CssClass;
        target.ComponentName = source.ComponentName;
        target.IsActive = source.IsActive;
        target.IsVisible = source.IsVisible;
    }
}
