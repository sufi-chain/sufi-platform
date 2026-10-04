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

    protected string ResolveMenuDisplayName(string? storedDisplayName, string? contextType = null, string? plainName = null)
    {
        return ResolveBusinessDisplayName(storedDisplayName, contextType, plainName);
    }

    protected string ResolveMenuItemDisplayName(string? storedDisplayName, string? contextType = null, string? plainName = null)
    {
        return ResolveBusinessDisplayName(storedDisplayName, contextType, plainName);
    }

    protected string ResolveMenuDisplayName(MenuListDto menu) =>
        ResolveMenuDisplayName(menu.DisplayName, menu.ContextType, menu.Name);

    protected string ResolveMenuDisplayName(MenuDto menu) =>
        ResolveMenuDisplayName(menu.DisplayName, menu.ContextType, menu.Name);

    protected string ResolveMenuItemDisplayName(MenuItemDto item, string? contextType = null) =>
        ResolveMenuItemDisplayName(item.DisplayName, contextType, item.Name);

    protected string ResolveMenuItemDisplayName(MenuItemTreeDto item, string? contextType = null) =>
        ResolveMenuItemDisplayName(item.DisplayName, contextType, item.Name);

    protected string ResolveContextType(string? contextType)
    {
        if (string.IsNullOrWhiteSpace(contextType))
        {
            return string.Empty;
        }

        var localized = L[$"ContextType:{contextType}"];
        return localized.ResourceNotFound ? contextType : localized.Value ?? contextType;
    }

    private string ResolveBusinessDisplayName(string? storedDisplayName, string? contextType, string? plainName)
    {
        string? localized = null;
        if (BusinessLocalizationHelper.IsBusinessLocalizationKey(storedDisplayName))
        {
            string? menuKey = null;
            if (BusinessLocalizationHelper.TryExtractSeededMenuKey(storedDisplayName!, out var extractedMenuKey))
            {
                menuKey = extractedMenuKey;
            }

            var resourceName = MenuLocalizationRegistry.GetResourceName(menuKey, contextType);
            localized = BusinessLocalizationHelper.ResolveText(
                StringLocalizerFactory,
                resourceName,
                storedDisplayName,
                fallback: string.Empty);
        }

        return BusinessTextEditorStorage.ResolveDisplayName(storedDisplayName, plainName, localized);
    }
}