using Volo.Abp;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Scheduling;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public class FreeBusyService : IFreeBusyService, ITransientDependency
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly ICalendarEventService _calendarEventService;
    private readonly IAvailabilityCalendarService _availabilityCalendarService;

    public FreeBusyService(
        ICalendarRepository calendarRepository,
        ICalendarEventService calendarEventService,
        IAvailabilityCalendarService availabilityCalendarService)
    {
        _calendarRepository = calendarRepository;
        _calendarEventService = calendarEventService;
        _availabilityCalendarService = availabilityCalendarService;
    }

    public virtual async Task<FreeBusyResult> GetFreeBusyAsync(IReadOnlyList<Guid> calendarIds, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        if (calendarIds.Count == 0 || utcTo <= utcFrom)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidFreeBusyWindow);
        }

        var busyBlocks = new List<BusyBlock>();
        var freeSlots = new List<FreeSlot>();

        foreach (var calendarId in calendarIds.Distinct())
        {
            var calendar = await _calendarRepository.GetAsync(calendarId, cancellationToken: ct);
            var occurrences = await _calendarEventService.ExpandAsync(calendarId, utcFrom, utcTo, ct);
            var activeOccurrences = occurrences
                .Where(x => x.Status != EventStatus.Cancelled)
                .OrderBy(x => x.StartUtc)
                .ThenBy(x => x.EndUtc)
                .ToList();

            busyBlocks.AddRange(BuildBusyBlocks(calendar.Id, activeOccurrences));
            freeSlots.AddRange(await BuildFreeSlotsAsync(calendar, activeOccurrences, utcFrom, utcTo, ct));
        }

        return new FreeBusyResult(
            DateTime.SpecifyKind(utcFrom, DateTimeKind.Utc),
            DateTime.SpecifyKind(utcTo, DateTimeKind.Utc),
            busyBlocks.OrderBy(x => x.StartUtc).ThenBy(x => x.EndUtc).ToList(),
            freeSlots.OrderBy(x => x.StartUtc).ThenBy(x => x.EndUtc).ToList());
    }

    private static IReadOnlyList<BusyBlock> BuildBusyBlocks(Guid calendarId, IReadOnlyList<EventOccurrence> occurrences)
    {
        var points = occurrences
            .SelectMany(x => new[]
            {
                new CapacityPoint(x.StartUtc, 1),
                new CapacityPoint(x.EndUtc, -1)
            })
            .OrderBy(x => x.Utc)
            .ThenBy(x => x.Delta)
            .ToList();

        var blocks = new List<BusyBlock>();
        var busyCount = 0;
        DateTime? segmentStart = null;

        foreach (var point in points)
        {
            if (segmentStart.HasValue && point.Utc > segmentStart.Value && busyCount > 0)
            {
                blocks.Add(new BusyBlock(calendarId, segmentStart.Value, point.Utc, busyCount));
            }

            busyCount += point.Delta;
            segmentStart = point.Utc;
        }

        return blocks;
    }

    private async Task<IReadOnlyList<FreeSlot>> BuildFreeSlotsAsync(Calendars.Calendar calendar, IReadOnlyList<EventOccurrence> occurrences, DateTime utcFrom, DateTime utcTo, CancellationToken ct)
    {
        var windows = await BuildAvailabilityWindowsAsync(calendar, utcFrom, utcTo, ct);
        var busyBlocks = BuildBusyBlocks(calendar.Id, occurrences);
        return SubtractBusy(windows, busyBlocks, calendar.Id);
    }

    private async Task<IReadOnlyList<FreeSlot>> BuildAvailabilityWindowsAsync(Calendars.Calendar calendar, DateTime utcFrom, DateTime utcTo, CancellationToken ct)
    {
        var availabilityCalendarId = calendar.Id;
        var windows = new List<FreeSlot>();
        var cursor = utcFrom;

        while (cursor < utcTo)
        {
            var openAtCursor = await _availabilityCalendarService.IsOpenAtAsync(availabilityCalendarId, cursor, ct);
            var start = openAtCursor ? cursor : await _availabilityCalendarService.NextOpenAtAsync(availabilityCalendarId, cursor, ct);
            if (!openAtCursor && start <= cursor)
            {
                start = cursor.AddMinutes(1);
            }
            if (start >= utcTo)
            {
                break;
            }

            var close = await _availabilityCalendarService.NextCloseAtAsync(availabilityCalendarId, start, ct);
            if (close <= start)
            {
                cursor = start.AddMinutes(1);
                continue;
            }

            var end = close < utcTo ? close : utcTo;
            if (end > start)
            {
                windows.Add(new FreeSlot(calendar.Id, start, end));
            }

            cursor = end <= start ? start.AddMinutes(1) : end;
        }

        return windows;
    }

    private static IReadOnlyList<FreeSlot> SubtractBusy(IReadOnlyList<FreeSlot> windows, IReadOnlyList<BusyBlock> busyBlocks, Guid calendarId)
    {
        var freeSlots = new List<FreeSlot>();
        foreach (var window in windows)
        {
            var cursor = window.StartUtc;
            foreach (var busy in busyBlocks.Where(x => x.EndUtc > window.StartUtc && x.StartUtc < window.EndUtc).OrderBy(x => x.StartUtc))
            {
                var busyStart = busy.StartUtc > window.StartUtc ? busy.StartUtc : window.StartUtc;
                var busyEnd = busy.EndUtc < window.EndUtc ? busy.EndUtc : window.EndUtc;

                if (busyStart > cursor)
                {
                    freeSlots.Add(new FreeSlot(calendarId, cursor, busyStart));
                }

                if (busyEnd > cursor)
                {
                    cursor = busyEnd;
                }
            }

            if (cursor < window.EndUtc)
            {
                freeSlots.Add(new FreeSlot(calendarId, cursor, window.EndUtc));
            }
        }

        return freeSlots;
    }

    private sealed record CapacityPoint(DateTime Utc, int Delta);
}
