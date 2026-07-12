using Shouldly;
using SufiChain.SufiPlatform.Calendar.Events;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Scheduling;

public class RecurrenceCalculatorTests
{
    private readonly RecurrenceCalculator _calculator = new();

    [Fact]
    public void Should_Expand_Daily_Recurrence_Over_Window()
    {
        var calendarEvent = CreateEvent(Utc(2026, 6, 8, 9), Utc(2026, 6, 8, 10));
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=DAILY;COUNT=3");

        var occurrences = _calculator.Expand(calendarEvent, Utc(2026, 6, 8), Utc(2026, 6, 12));

        occurrences.Count.ShouldBe(3);
        occurrences.Select(x => x.StartUtc).ShouldBe(new[]
        {
            Utc(2026, 6, 8, 9),
            Utc(2026, 6, 9, 9),
            Utc(2026, 6, 10, 9)
        });
    }

    [Fact]
    public void Should_Expand_Weekly_Recurrence_Over_Window()
    {
        var calendarEvent = CreateEvent(Utc(2026, 6, 8, 9), Utc(2026, 6, 8, 10));
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=WEEKLY;INTERVAL=2;COUNT=3");

        var occurrences = _calculator.Expand(calendarEvent, Utc(2026, 6, 1), Utc(2026, 7, 31));

        occurrences.Select(x => x.StartUtc).ShouldBe(new[]
        {
            Utc(2026, 6, 8, 9),
            Utc(2026, 6, 22, 9),
            Utc(2026, 7, 6, 9)
        });
    }

    [Fact]
    public void Should_Expand_Monthly_Recurrence_Over_Window()
    {
        var calendarEvent = CreateEvent(Utc(2026, 1, 15, 9), Utc(2026, 1, 15, 10));
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=MONTHLY;COUNT=3");

        var occurrences = _calculator.Expand(calendarEvent, Utc(2026, 1, 1), Utc(2026, 5, 1));

        occurrences.Select(x => x.StartUtc).ShouldBe(new[]
        {
            Utc(2026, 1, 15, 9),
            Utc(2026, 2, 15, 9),
            Utc(2026, 3, 15, 9)
        });
    }

    [Fact]
    public void Should_Apply_Cancel_And_Move_Occurrence_Exceptions()
    {
        var calendarEvent = CreateEvent(Utc(2026, 6, 8, 9), Utc(2026, 6, 8, 10));
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=DAILY;COUNT=4");
        calendarEvent.CancelOccurrence(Guid.NewGuid(), Utc(2026, 6, 9, 9));
        calendarEvent.MoveOccurrence(Guid.NewGuid(), Utc(2026, 6, 10, 9), Utc(2026, 6, 10, 12), Utc(2026, 6, 10, 13));

        var occurrences = _calculator.Expand(calendarEvent, Utc(2026, 6, 8), Utc(2026, 6, 13));

        occurrences.Count.ShouldBe(3);
        occurrences.Select(x => x.OriginalStartUtc).ShouldBe(new[]
        {
            Utc(2026, 6, 8, 9),
            Utc(2026, 6, 10, 9),
            Utc(2026, 6, 11, 9)
        });
        occurrences[1].StartUtc.ShouldBe(Utc(2026, 6, 10, 12));
        occurrences[1].EndUtc.ShouldBe(Utc(2026, 6, 10, 13));
    }

    [Fact]
    public void Should_Expand_Dst_Spring_Forward_By_Local_Time()
    {
        var calendarEvent = CreateEvent(
            new DateTime(2026, 3, 7, 7, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 7, 8, 30, 0, DateTimeKind.Utc),
            "America/New_York");
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=DAILY;COUNT=3");

        var occurrences = _calculator.Expand(
            calendarEvent,
            new DateTime(2026, 3, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 11, 0, 0, 0, DateTimeKind.Utc));

        occurrences.Select(x => x.StartUtc).ShouldBe(new[]
        {
            new DateTime(2026, 3, 7, 7, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 8, 7, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 9, 6, 30, 0, DateTimeKind.Utc)
        });
    }

    private static CalendarEvent CreateEvent(DateTime startUtc, DateTime endUtc, string timeZoneId = "UTC")
    {
        return new CalendarEvent(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            "Planning",
            startUtc,
            endUtc,
            false,
            timeZoneId);
    }

    private static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0)
    {
        return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }
}
