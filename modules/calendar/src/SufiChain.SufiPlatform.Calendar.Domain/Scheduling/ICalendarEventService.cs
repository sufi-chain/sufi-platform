using SufiChain.SufiPlatform.Calendar.Events;

namespace SufiChain.SufiPlatform.Calendar.Scheduling;

public interface ICalendarEventService
{
    Task<IReadOnlyList<EventOccurrence>> ExpandAsync(Guid calendarId, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);
}
