namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Maps an internal menu item onto the anonymous response and drops items that require a permission.
/// </summary>
public static class PublicMenuPresentation
{
    public static bool IsAnonymous(MenuItemDto? item) =>
        item != null && string.IsNullOrWhiteSpace(item.PermissionName);

    public static PublicMenuItemDto? ToItem(MenuItemDto? item)
    {
        if (!IsAnonymous(item) || item == null)
        {
            return null;
        }

        var copy = new PublicMenuItemDto();
        Copy(item, copy);
        return copy;
    }

    public static List<PublicMenuItemTreeDto> ToTree(IEnumerable<MenuItemTreeDto> items)
    {
        var result = new List<PublicMenuItemTreeDto>();
        foreach (var item in items)
        {
            if (!IsAnonymous(item))
            {
                continue;
            }

            var copy = new PublicMenuItemTreeDto();
            Copy(item, copy);
            copy.Children = ToTree(item.Children);
            result.Add(copy);
        }

        return result;
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
