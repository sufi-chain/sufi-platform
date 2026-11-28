namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public static class CalendarNewEventCalendar
{
    public static Guid Resolve(Guid requestedCalendarId, Guid defaultCalendarId) =>
        requestedCalendarId != Guid.Empty ? requestedCalendarId : defaultCalendarId;

    public static bool RequiresSelection(Guid resolvedCalendarId) => resolvedCalendarId == Guid.Empty;
}
