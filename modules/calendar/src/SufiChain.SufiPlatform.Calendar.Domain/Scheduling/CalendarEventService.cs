using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Scheduling;

public class CalendarEventService : ICalendarEventService, ITransientDependency
{
    private readonly ICalendarEventRepository _eventRepository;
    private readonly RecurrenceCalculator _recurrenceCalculator;
    private readonly ICalendarOccurrenceExpansionCache _occurrenceCache;

    public CalendarEventService(
        ICalendarEventRepository eventRepository,
        RecurrenceCalculator recurrenceCalculator,
        ICalendarOccurrenceExpansionCache occurrenceCache)
    {
        _eventRepository = eventRepository;
        _recurrenceCalculator = recurrenceCalculator;
        _occurrenceCache = occurrenceCache;
    }

    public virtual async Task<IReadOnlyList<EventOccurrence>> ExpandAsync(Guid calendarId, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        return await _occurrenceCache.GetOrAddAsync(
            calendarId,
            utcFrom,
            utcTo,
            async () =>
            {
                var events = await _eventRepository.GetListInWindowAsync(calendarId, utcFrom, utcTo, ct);
                return (IReadOnlyList<EventOccurrence>)events
                    .SelectMany(calendarEvent => _recurrenceCalculator.Expand(calendarEvent, utcFrom, utcTo))
                    .OrderBy(x => x.StartUtc)
                    .ThenBy(x => x.EndUtc)
                    .ToList();
            },
            ct);
    }
}
