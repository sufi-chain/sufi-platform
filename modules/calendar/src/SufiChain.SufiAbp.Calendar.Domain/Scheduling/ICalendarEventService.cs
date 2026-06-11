using SufiChain.SufiAbp.Calendar.Events;

namespace SufiChain.SufiAbp.Calendar.Scheduling;

public interface ICalendarEventService
{
    Task<IReadOnlyList<EventOccurrence>> ExpandAsync(Guid calendarId, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);
}
