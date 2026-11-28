using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor;

/// <summary>
/// Base class for Calendar Hooshvare Blazor components. Reuses the Calendar localization
/// resource so no new resource module is introduced.
/// </summary>
public abstract class CalendarHooshvareComponentBase : SufiComponentBase
{
    protected CalendarHooshvareComponentBase()
    {
        LocalizationResource = typeof(CalendarResource);
    }
}
