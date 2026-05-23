using SufiChain.SufiAbp.SettingManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.SettingManagement.Blazor;

/// <summary>
/// Base class for Blazor components in the Setting Management module.
/// Provides module localization via SettingManagementResource.
/// </summary>
public abstract class SettingManagementComponentBase : SufiAbpComponentBase
{
    protected SettingManagementComponentBase()
    {
        LocalizationResource = typeof(SufiAbpSettingManagementResource);
    }
}
