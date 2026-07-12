using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Menus.Localization;
using SufiChain.SufiPlatform.Menus.Menus;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Menus.Blazor;

public abstract class MenusComponentBase : SufiComponentBase
{
    protected MenusComponentBase()
    {
        LocalizationResource = typeof(SufiMenusResource);
    }

    protected string ResolveMenuDisplayName(string? storedDisplayName, string? contextType = null)
    {
        return ResolveBusinessDisplayName(storedDisplayName, contextType);
    }

    protected string ResolveMenuItemDisplayName(string? storedDisplayName, string? contextType = null)
    {
        return ResolveBusinessDisplayName(storedDisplayName, contextType);
    }

    protected string ResolveMenuDisplayName(MenuListDto menu) =>
        ResolveMenuDisplayName(menu.DisplayName, menu.ContextType);

    protected string ResolveMenuDisplayName(MenuDto menu) =>
        ResolveMenuDisplayName(menu.DisplayName, menu.ContextType);

    protected string ResolveMenuItemDisplayName(MenuItemDto item, string? contextType = null) =>
        ResolveMenuItemDisplayName(item.DisplayName, contextType);

    protected string ResolveMenuItemDisplayName(MenuItemTreeDto item, string? contextType = null) =>
        ResolveMenuItemDisplayName(item.DisplayName, contextType);

    protected string ResolveContextType(string? contextType)
    {
        if (string.IsNullOrWhiteSpace(contextType))
        {
            return string.Empty;
        }

        var localized = L[$"ContextType:{contextType}"];
        return localized.ResourceNotFound ? contextType : localized.Value ?? contextType;
    }

    private string ResolveBusinessDisplayName(string? storedDisplayName, string? contextType)
    {
        if (string.IsNullOrWhiteSpace(storedDisplayName))
        {
            return string.Empty;
        }

        if (!BusinessLocalizationHelper.IsBusinessLocalizationKey(storedDisplayName))
        {
            return storedDisplayName;
        }

        string? menuKey = null;
        if (BusinessLocalizationHelper.TryExtractSeededMenuKey(storedDisplayName, out var extractedMenuKey))
        {
            menuKey = extractedMenuKey;
        }

        var resourceName = MenuLocalizationRegistry.GetResourceName(menuKey, contextType);
        return BusinessLocalizationHelper.ResolveText(
            StringLocalizerFactory,
            resourceName,
            storedDisplayName,
            storedDisplayName);
    }
}