using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Menus.Localization;
using SufiChain.SufiPlatform.Menus.Menus;
using SufiChain.SufiPlatform.UI.Blazor;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Menus.Blazor;

public abstract class MenusComponentBase : SufiComponentBase
{
    private IMenuAppService? _menuLabelAppService;
    private IOptions<AbpLocalizationOptions>? _localizationOptions;
    private IJSRuntime? _jsRuntime;

    protected string MenuDefaultCulture { get; private set; } = "fa";

    protected MenusComponentBase()
    {
        LocalizationResource = typeof(SufiMenusResource);
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var cultures = await LazyGetRequiredService(ref _menuLabelAppService).GetLabelCulturesAsync();
            var selected = cultures.FirstOrDefault(x => x.IsDefault);
            if (!string.IsNullOrWhiteSpace(selected?.CultureName))
            {
                MenuDefaultCulture = selected.CultureName;
            }
        }
        catch (Exception)
        {
            MenuDefaultCulture = "fa";
        }

        await base.OnInitializedAsync();
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
        string? menuKey = null;
        if (!string.IsNullOrWhiteSpace(storedDisplayName)
            && BusinessLocalizationHelper.TryExtractSeededMenuKey(storedDisplayName, out var extractedMenuKey))
        {
            menuKey = extractedMenuKey;
        }

        var resources = MenuLocalizationRegistry.GetReadResourceNames(menuKey, contextType);
        var options = LazyGetRequiredService(ref _localizationOptions).Value;
        return MenuDisplayNamePlanner.ResolvePublicLabel(
            storedDisplayName,
            plainName,
            CultureInfo.CurrentUICulture.Name,
            MenuDefaultCulture,
            culture => MenuDisplayNamePlanner.LookupExact(
                options,
                resources,
                storedDisplayName,
                culture));
    }

    protected async Task CopyTextAsync(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        try
        {
            var js = LazyGetRequiredService(ref _jsRuntime);
            await js.InvokeVoidAsync("navigator.clipboard.writeText", value);
        }
        catch (Exception)
        {
            // Clipboard access can be refused before the page is interactive.
        }
    }
}