using SufiChain.SufiAbp.LocalizationManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.LocalizationManagement.Blazor;

/// <summary>
/// Base class for Blazor components in the Localization Management module.
/// Provides module localization via LocalizationManagementResource.
/// </summary>
public abstract class LocalizationManagementComponentBase : SufiAbpComponentBase
{
    protected LocalizationManagementComponentBase()
    {
        LocalizationResource = typeof(SufiAbpLocalizationManagementResource);
    }
}
