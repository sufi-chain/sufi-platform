using SufiChain.SufiAbp.Calendar.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.Calendar.Blazor;

public abstract class CalendarComponentBase : SufiAbpComponentBase
{
    protected CalendarComponentBase()
    {
        LocalizationResource = typeof(CalendarResource);
    }
}
