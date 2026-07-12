using SufiChain.SufiPlatform.ShortLinks.Localization;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.ShortLinks;

public abstract class ShortLinksAppService : SufiApplicationService
{
    protected ShortLinksAppService()
    {
        LocalizationResource = typeof(SufiShortLinksResource);
        ObjectMapperContext = typeof(SufiShortLinksApplicationModule);
    }
}