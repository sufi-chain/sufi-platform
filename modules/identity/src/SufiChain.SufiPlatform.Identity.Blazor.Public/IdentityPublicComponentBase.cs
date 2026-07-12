using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Identity.Blazor.Public;

public abstract class IdentityPublicComponentBase : SufiComponentBase
{
    protected IdentityPublicComponentBase()
    {
        LocalizationResource = typeof(SufiIdentityResource);
    }
}
