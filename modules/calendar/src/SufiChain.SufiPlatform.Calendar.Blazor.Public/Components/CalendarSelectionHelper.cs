using SufiChain.SufiPlatform.Calendar.Calendars;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public static class CalendarSelectionHelper
{
    public static CalendarLookupDto? ResolveDefaultSelection(
        IReadOnlyList<CalendarLookupDto> options,
        Guid selectedCalendarId,
        Guid? initialCalendarId)
    {
        if (selectedCalendarId != Guid.Empty)
        {
            return options.FirstOrDefault(x => x.Id == selectedCalendarId);
        }

        if (initialCalendarId.HasValue)
        {
            return options.FirstOrDefault(x => x.Id == initialCalendarId.Value);
        }

        return options.FirstOrDefault(x => x.Kind == CalendarKind.Personal)
            ?? options.FirstOrDefault(x => x.IsDefault)
            ?? options.FirstOrDefault();
    }
}
