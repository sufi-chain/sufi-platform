using SufiChain.SufiAbp.MenuManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.MenuManagement.Blazor;

public abstract class MenuManagementComponentBase : SufiAbpComponentBase
{
    protected MenuManagementComponentBase()
    {
        LocalizationResource = typeof(SufiAbpMenuManagementResource);
    }
}
