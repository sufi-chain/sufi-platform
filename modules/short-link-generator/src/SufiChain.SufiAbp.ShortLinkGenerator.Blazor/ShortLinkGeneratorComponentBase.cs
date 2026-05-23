using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Blazor;

public abstract class ShortLinkGeneratorComponentBase : SufiAbpComponentBase
{
    protected ShortLinkGeneratorComponentBase()
    {
        LocalizationResource = typeof(SufiAbpShortLinkGeneratorResource);
    }
}
