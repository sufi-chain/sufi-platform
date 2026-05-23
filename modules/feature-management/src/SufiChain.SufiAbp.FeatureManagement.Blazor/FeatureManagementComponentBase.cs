using SufiChain.SufiAbp.FeatureManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.FeatureManagement.Blazor;

/// <summary>
/// Base class for Blazor components in the Feature Management module.
/// Provides module localization via FeatureManagementResource.
/// </summary>
public abstract class FeatureManagementComponentBase : SufiAbpComponentBase
{
    protected FeatureManagementComponentBase()
    {
        LocalizationResource = typeof(SufiAbpFeatureManagementResource);
    }
}
