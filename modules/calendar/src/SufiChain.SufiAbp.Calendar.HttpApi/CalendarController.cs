using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.Calendar.Localization;

namespace SufiChain.SufiAbp.Calendar;

public abstract class CalendarController : SufiAbpControllerBase
{
    protected CalendarController()
    {
        LocalizationResource = typeof(CalendarResource);
    }
}
