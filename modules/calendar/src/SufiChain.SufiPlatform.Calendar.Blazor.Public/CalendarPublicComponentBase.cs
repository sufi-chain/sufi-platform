using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public;

public abstract class CalendarPublicComponentBase : SufiComponentBase
{
    protected CalendarPublicComponentBase()
    {
        LocalizationResource = typeof(CalendarResource);
    }
}
