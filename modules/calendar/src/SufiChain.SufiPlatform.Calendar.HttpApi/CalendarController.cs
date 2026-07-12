using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Calendar.Localization;

namespace SufiChain.SufiPlatform.Calendar;

public abstract class CalendarController : SufiControllerBase
{
    protected CalendarController()
    {
        LocalizationResource = typeof(CalendarResource);
    }
}
