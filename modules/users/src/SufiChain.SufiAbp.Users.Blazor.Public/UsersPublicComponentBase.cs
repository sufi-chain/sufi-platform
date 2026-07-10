using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.Users.Localization;

namespace SufiChain.SufiAbp.Users.Blazor.Public;

public abstract class UsersPublicComponentBase : SufiAbpComponentBase
{
    protected UsersPublicComponentBase()
    {
        LocalizationResource = typeof(SufiAbpUsersResource);
    }
}
