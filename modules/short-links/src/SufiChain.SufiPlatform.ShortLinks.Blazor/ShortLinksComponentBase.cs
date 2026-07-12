using SufiChain.SufiPlatform.ShortLinks.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.ShortLinks.Blazor;

public abstract class ShortLinksComponentBase : SufiComponentBase
{
    protected ShortLinksComponentBase()
    {
        LocalizationResource = typeof(SufiShortLinksResource);
    }
}