using SufiChain.SufiAbp.LocalizationManagement.Localization;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.LocalizationManagement;

public abstract class LocalizationManagementController : SufiAbpControllerBase
{
    protected LocalizationManagementController()
    {
        LocalizationResource = typeof(SufiAbpLocalizationManagementResource);
    }
}
