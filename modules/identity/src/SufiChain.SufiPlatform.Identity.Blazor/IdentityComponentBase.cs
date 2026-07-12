using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Identity.Blazor;

/// <summary>
/// Base class for Blazor components in the Identity module.
/// Provides module localization via IdentityResource.
/// </summary>
public abstract class IdentityComponentBase : SufiComponentBase
{
    protected IdentityComponentBase()
    {
        LocalizationResource = typeof(SufiIdentityResource);
    }
}
