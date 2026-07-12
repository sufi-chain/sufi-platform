using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Localization.ExternalStore;

/// <summary>
/// A localization resource that is loaded from an external source (database).
/// </summary>
public class ExternalLocalizationResource : LocalizationResourceBase
{
    public ExternalLocalizationResource(
        string resourceName,
        string? defaultCultureName = null,
        ILocalizationResourceContributor? initialContributor = null)
        : base(resourceName, defaultCultureName, initialContributor)
    {
    }
}
