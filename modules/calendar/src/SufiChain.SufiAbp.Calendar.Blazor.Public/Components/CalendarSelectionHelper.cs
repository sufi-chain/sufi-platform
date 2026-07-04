using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public.Components;

internal static class CalendarSelectionHelper
{
    internal static CalendarLookupDto? ResolveDefaultSelection(
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
