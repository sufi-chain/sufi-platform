using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Calendar.Blazor;

public abstract class CalendarComponentBase : SufiComponentBase
{
    protected CalendarComponentBase()
    {
        LocalizationResource = typeof(CalendarResource);
    }
}
