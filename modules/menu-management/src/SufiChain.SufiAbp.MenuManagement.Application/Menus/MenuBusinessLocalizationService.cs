using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.MenuManagement.Menus;

public class MenuBusinessLocalizationService : ITransientDependency
{
    protected IStringLocalizerFactory StringLocalizerFactory { get; }

    public MenuBusinessLocalizationService(IStringLocalizerFactory stringLocalizerFactory)
    {
        StringLocalizerFactory = stringLocalizerFactory;
    }

    public virtual string ResolveMenuDisplayName(string? storedDisplayName, string? contextType = null)
    {
        return ResolveDisplayName(storedDisplayName, contextType);
    }

    public virtual string ResolveMenuItemDisplayName(string? storedDisplayName, string? contextType = null)
    {
        return ResolveDisplayName(storedDisplayName, contextType);
    }

    protected virtual string ResolveDisplayName(string? storedDisplayName, string? contextType)
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
