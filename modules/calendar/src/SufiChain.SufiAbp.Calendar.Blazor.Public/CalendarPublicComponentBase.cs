using SufiChain.SufiAbp.Calendar.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public;

public abstract class CalendarPublicComponentBase : SufiAbpComponentBase
{
    protected CalendarPublicComponentBase()
    {
        LocalizationResource = typeof(CalendarResource);
    }
}
