using NSubstitute;
using Shouldly;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using SufiChain.SufiAbp.Calendar.Scheduling;
using Xunit;
using CalendarAggregate = SufiChain.SufiAbp.Calendar.Calendars.Calendar;

namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public class FreeBusyServiceTests
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly ICalendarEventService _calendarEventService;
    private readonly IAvailabilityCalendarService _availabilityCalendarService;
    private readonly FreeBusyService _service;

    public FreeBusyServiceTests()
    {
        _calendarRepository = Substitute.For<ICalendarRepository>();
        _calendarEventService = Substitute.For<ICalendarEventService>();
        _availabilityCalendarService = Substitute.For<IAvailabilityCalendarService>();
        _service = new FreeBusyService(_calendarRepository, _calendarEventService, _availabilityCalendarService);
    }

    [Fact]
    public async Task Should_Calculate_Busy_Blocks_And_Free_Slots_For_Exclusive_Calendar()
    {
        var calendar = CreateCalendar(maxConcurrent: 1);
        var from = Utc(2026, 6, 8, 9);
        var to = Utc(2026, 6, 8, 17);
        var occurrences = new[]
        {
            CreateOccurrence(calendar.Id, Utc(2026, 6, 8, 10), Utc(2026, 6, 8, 11)),
            CreateOccurrence(calendar.Id, Utc(2026, 6, 8, 13), Utc(2026, 6, 8, 14))
        };

        SetupCalendar(calendar, from, to, occurrences);

        var result = await _service.GetFreeBusyAsync(new[] { calendar.Id }, from, to);

        result.BusyBlocks.Select(x => (x.StartUtc, x.EndUtc, x.BusyCount, x.IsCapacityFull)).ShouldBe(new[]
        {
            (Utc(2026, 6, 8, 10), Utc(2026, 6, 8, 11), 1, true),
            (Utc(2026, 6, 8, 13), Utc(2026, 6, 8, 14), 1, true)
        });
        result.FreeSlots.Select(x => (x.StartUtc, x.EndUtc)).ShouldBe(new[]
        {
            (Utc(2026, 6, 8, 9), Utc(2026, 6, 8, 10)),
            (Utc(2026, 6, 8, 11), Utc(2026, 6, 8, 13)),
            (Utc(2026, 6, 8, 14), Utc(2026, 6, 8, 17))
        });
    }

    [Fact]
    public async Task Should_Count_Overlaps_And_Only_Block_Free_Slots_When_Capacity_Is_Full()
    {
        var calendar = CreateCalendar(maxConcurrent: 2);
        var from = Utc(2026, 6, 8, 9);
        var to = Utc(2026, 6, 8, 13);
        var occurrences = new[]
        {
            CreateOccurrence(calendar.Id, Utc(2026, 6, 8, 10), Utc(2026, 6, 8, 12)),
            CreateOccurrence(calendar.Id, Utc(2026, 6, 8, 11), Utc(2026, 6, 8, 12))
        };

        SetupCalendar(calendar, from, to, occurrences);

        var result = await _service.GetFreeBusyAsync(new[] { calendar.Id }, from, to);

        result.BusyBlocks.Select(x => (x.StartUtc, x.EndUtc, x.BusyCount, x.IsCapacityFull)).ShouldBe(new[]
        {
            (Utc(2026, 6, 8, 10), Utc(2026, 6, 8, 11), 1, false),
            (Utc(2026, 6, 8, 11), Utc(2026, 6, 8, 12), 2, true)
        });
        result.FreeSlots.Select(x => (x.StartUtc, x.EndUtc)).ShouldBe(new[]
        {
            (Utc(2026, 6, 8, 9), Utc(2026, 6, 8, 11)),
            (Utc(2026, 6, 8, 12), Utc(2026, 6, 8, 13))
        });
    }

    [Fact]
    public async Task Should_Honor_Availability_Windows_When_Calculating_Free_Slots()
    {
        var calendar = CreateCalendar(maxConcurrent: 1);
        var from = Utc(2026, 6, 8, 7);
        var to = Utc(2026, 6, 8, 18);
        var open = Utc(2026, 6, 8, 9);
        var close = Utc(2026, 6, 8, 17);
        var occurrences = new[]
        {
            CreateOccurrence(calendar.Id, Utc(2026, 6, 8, 10), Utc(2026, 6, 8, 11))
        };

        SetupCalendar(calendar, from, to, occurrences, open, close);

        var result = await _service.GetFreeBusyAsync(new[] { calendar.Id }, from, to);

        result.FreeSlots.Select(x => (x.StartUtc, x.EndUtc)).ShouldBe(new[]
        {
            (Utc(2026, 6, 8, 9), Utc(2026, 6, 8, 10)),
            (Utc(2026, 6, 8, 11), Utc(2026, 6, 8, 17))
        });
    }

    private void SetupCalendar(
        CalendarAggregate calendar,
        DateTime from,
        DateTime to,
        IReadOnlyList<EventOccurrence> occurrences,
        DateTime? open = null,
        DateTime? close = null)
    {
        open ??= from;
        close ??= to;
        _calendarRepository.GetAsync(calendar.Id, cancellationToken: Arg.Any<CancellationToken>()).Returns(calendar);
        _calendarEventService.ExpandAsync(calendar.Id, from, to, Arg.Any<CancellationToken>()).Returns(occurrences);
        _availabilityCalendarService.IsOpenAtAsync(calendar.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var instant = callInfo.ArgAt<DateTime>(1);
                return instant >= open.Value && instant < close.Value;
            });
        _availabilityCalendarService.NextOpenAtAsync(calendar.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(open.Value);
        _availabilityCalendarService.NextCloseAtAsync(calendar.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(close.Value);
    }

    private static CalendarAggregate CreateCalendar(int? maxConcurrent)
    {
        return new CalendarAggregate(
            Guid.NewGuid(),
            null,
            "Team calendar",
            CalendarKind.Shared,
            "UTC",
            maxConcurrent: maxConcurrent);
    }

    private static EventOccurrence CreateOccurrence(Guid calendarId, DateTime startUtc, DateTime endUtc)
    {
        return new EventOccurrence(
            Guid.NewGuid(),
            calendarId,
            "Busy",
            startUtc,
            startUtc,
            endUtc,
            false,
            "UTC",
            EventStatus.Confirmed,
            null,
            null,
            null,
            null,
            null);
    }

    private static DateTime Utc(int year, int month, int day, int hour)
    {
        return new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc);
    }
}
