using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public abstract class ShortLinkGeneratorController : SufiAbpControllerBase
{
    protected ShortLinkGeneratorController()
    {
        LocalizationResource = typeof(SufiAbpShortLinkGeneratorResource);
    }
}
