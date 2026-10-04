using System.Globalization;
using SufiChain.SufiPlatform.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Menus.Menus;

public class MenuBusinessLocalizationService : ITransientDependency
{
    private readonly MenuLabelLocalization _labels;

    public MenuBusinessLocalizationService(MenuLabelLocalization labels)
    {
        _labels = labels;
    }

    public virtual string ResolveMenuDisplayName(
        string? storedDisplayName,
        string? contextType = null,
        string? plainName = null,
        string? requestedCulture = null,
        string? defaultCulture = null)
    {
        return Resolve(storedDisplayName, contextType, plainName, requestedCulture, defaultCulture);
    }

    public virtual string ResolveMenuItemDisplayName(
        string? storedDisplayName,
        string? contextType = null,
        string? plainName = null,
        string? requestedCulture = null,
        string? defaultCulture = null)
    {
        return Resolve(storedDisplayName, contextType, plainName, requestedCulture, defaultCulture);
    }

    private string Resolve(
        string? storedDisplayName,
        string? contextType,
        string? plainName,
        string? requestedCulture,
        string? defaultCulture)
    {
        var requested = string.IsNullOrWhiteSpace(requestedCulture)
            ? CultureInfo.CurrentUICulture.Name
            : requestedCulture;
        var fallbackCulture = string.IsNullOrWhiteSpace(defaultCulture) ? "fa" : defaultCulture;
        return _labels.Resolve(storedDisplayName, contextType, plainName, requested, fallbackCulture);
    }
}
