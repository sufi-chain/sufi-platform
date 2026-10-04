using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Menus.Menus;

public class MenuBusinessLocalizationService : ITransientDependency
{
    protected IStringLocalizerFactory StringLocalizerFactory { get; }

    public MenuBusinessLocalizationService(IStringLocalizerFactory stringLocalizerFactory)
    {
        StringLocalizerFactory = stringLocalizerFactory;
    }

    public virtual string ResolveMenuDisplayName(string? storedDisplayName, string? contextType = null, string? plainName = null)
    {
        return ResolveDisplayName(storedDisplayName, contextType, plainName);
    }

    public virtual string ResolveMenuItemDisplayName(string? storedDisplayName, string? contextType = null, string? plainName = null)
    {
        return ResolveDisplayName(storedDisplayName, contextType, plainName);
    }

    protected virtual string ResolveDisplayName(string? storedDisplayName, string? contextType, string? plainName)
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
