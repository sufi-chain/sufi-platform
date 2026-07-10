using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.Identity.Blazor.Public;

public abstract class IdentityPublicComponentBase : SufiAbpComponentBase
{
    protected IdentityPublicComponentBase()
    {
        LocalizationResource = typeof(SufiAbpIdentityResource);
    }
}
