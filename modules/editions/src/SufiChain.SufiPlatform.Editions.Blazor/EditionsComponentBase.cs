using SufiChain.SufiPlatform.Editions.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Editions.Blazor;

public abstract class EditionsComponentBase : SufiComponentBase
{
    public EditionsComponentBase()
    {
        LocalizationResource = typeof(EditionsResource);
    }
}
