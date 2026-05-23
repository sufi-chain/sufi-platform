using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public abstract class ShortLinkGeneratorAppService : ApplicationService
{
    protected ShortLinkGeneratorAppService()
    {
        LocalizationResource = typeof(SufiAbpShortLinkGeneratorResource);
        ObjectMapperContext = typeof(SufiAbpShortLinkGeneratorApplicationModule);
    }
}
