using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Calendar.Scheduling;

public class CalendarEventService : ICalendarEventService, ITransientDependency
{
    private readonly ICalendarEventRepository _eventRepository;
    private readonly RecurrenceCalculator _recurrenceCalculator;

    public CalendarEventService(ICalendarEventRepository eventRepository, RecurrenceCalculator recurrenceCalculator)
    {
        _eventRepository = eventRepository;
        _recurrenceCalculator = recurrenceCalculator;
    }

    public virtual async Task<IReadOnlyList<EventOccurrence>> ExpandAsync(Guid calendarId, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        var events = await _eventRepository.GetListInWindowAsync(calendarId, utcFrom, utcTo, ct);
        return events
            .SelectMany(calendarEvent => _recurrenceCalculator.Expand(calendarEvent, utcFrom, utcTo))
            .OrderBy(x => x.StartUtc)
            .ThenBy(x => x.EndUtc)
            .ToList();
    }
}
