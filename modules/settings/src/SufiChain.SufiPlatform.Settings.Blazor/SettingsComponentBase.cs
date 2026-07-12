using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Settings.Blazor;

/// <summary>
/// Base class for Blazor components in the Setting Management module.
/// Provides module localization via SettingsResource.
/// </summary>
public abstract class SettingsComponentBase : SufiComponentBase
{
    protected SettingsComponentBase()
    {
        LocalizationResource = typeof(SufiSettingsResource);
    }
}
