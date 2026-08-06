using SufiChain.SufiPlatform.Localization.Localization;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.Localization;

public abstract class LocalizationController : SufiControllerBase
{
    protected LocalizationController()
    {
        LocalizationResource = typeof(SufiLocalizationResource);
    }
}
