using SufiChain.SufiPlatform.ShortLinks.Localization;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.ShortLinks;

public abstract class ShortLinksController : SufiControllerBase
{
    protected ShortLinksController()
    {
        LocalizationResource = typeof(SufiShortLinksResource);
    }
}