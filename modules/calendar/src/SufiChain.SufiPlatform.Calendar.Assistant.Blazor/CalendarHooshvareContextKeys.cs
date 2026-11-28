using SufiChain.SufiPlatform.Calendar.Calendars;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor;

/// <summary>
/// Keys inside chat message <see cref="SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare.HooshvareRequestContextMetadata"/> for Calendar hooshvare requests.
/// </summary>
public static class CalendarHooshvareContextKeys
{
    public const string CalendarId = "calendarId";

    public const string Calendars = CalendarVisibleCatalogJson.ContextKey;
}
