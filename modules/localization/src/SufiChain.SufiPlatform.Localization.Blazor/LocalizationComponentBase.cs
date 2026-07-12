using SufiChain.SufiPlatform.Localization.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Localization.Blazor;

/// <summary>
/// Base class for Blazor components in the Localization Management module.
/// Provides module localization via LocalizationResource.
/// </summary>
public abstract class LocalizationComponentBase : SufiComponentBase
{
    protected LocalizationComponentBase()
    {
        LocalizationResource = typeof(SufiLocalizationResource);
    }
}
