using SufiChain.SufiPlatform.Features.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Features.Blazor;

/// <summary>
/// Base class for Blazor components in the Feature Management module.
/// Provides module localization via FeaturesResource.
/// </summary>
public abstract class FeaturesComponentBase : SufiComponentBase
{
    protected FeaturesComponentBase()
    {
        LocalizationResource = typeof(SufiFeaturesResource);
    }
}
