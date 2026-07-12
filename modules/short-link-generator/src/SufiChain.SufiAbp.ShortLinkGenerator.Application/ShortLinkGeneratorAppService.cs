using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public abstract class ShortLinkGeneratorAppService : SufiAbpApplicationService
{
    protected ShortLinkGeneratorAppService()
    {
        LocalizationResource = typeof(SufiAbpShortLinkGeneratorResource);
        ObjectMapperContext = typeof(SufiAbpShortLinkGeneratorApplicationModule);
    }
}
