using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.Identity.Blazor;

/// <summary>
/// Base class for Blazor components in the Identity module.
/// Provides module localization via IdentityResource.
/// </summary>
public abstract class IdentityComponentBase : SufiAbpComponentBase
{
    protected IdentityComponentBase()
    {
        LocalizationResource = typeof(SufiAbpIdentityResource);
    }
}
