using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.Users.Localization;

namespace SufiChain.SufiPlatform.Users.Blazor.Public;

public abstract class UsersPublicComponentBase : SufiComponentBase
{
    protected UsersPublicComponentBase()
    {
        LocalizationResource = typeof(SufiUsersResource);
    }
}
