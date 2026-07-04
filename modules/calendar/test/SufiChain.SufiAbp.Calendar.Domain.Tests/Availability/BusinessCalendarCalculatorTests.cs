using Shouldly;
using SufiChain.SufiAbp.Calendar.Calendars;
using Xunit;

namespace SufiChain.SufiAbp.Calendar.Availability;

public class BusinessCalendarCalculatorTests
{
    private readonly BusinessCalendarCalculator _calculator = new();

    [Fact]
    public void Should_Treat_WorkingHours_Calendar_With_No_Rules_As_Always_Open()
    {
        var snapshot = CreateAlwaysOpenSnapshot();
        var start = Utc(2026, 6, 8, 10);
        var end = Utc(2026, 6, 10, 14);

        _calculator.IsOpenAt(snapshot, start).ShouldBeTrue();
        _calculator.ElapsedWorkingTime(snapshot, start, end).ShouldBe(end - start);
        _calculator.AddWorkingDuration(snapshot, start, TimeSpan.FromHours(52)).ShouldBe(end);
    }

    [Fact]
    public void Should_Exclude_Nights_And_Weekends_From_Elapsed_Working_Time()
    {
        var snapshot = CreateWeekdaySnapshot();
        var fridayAfternoon = Utc(2026, 6, 12, 15);
        var mondayMorning = Utc(2026, 6, 15, 11);

        _calculator.ElapsedWorkingTime(snapshot, fridayAfternoon, mondayMorning).ShouldBe(TimeSpan.FromHours(4));
    }

    [Fact]
    public void Should_Add_Working_Duration_Across_Close_And_Weekend_Boundaries()
    {
        var snapshot = CreateWeekdaySnapshot();
        var fridayAfternoon = Utc(2026, 6, 12, 16);

        _calculator.AddWorkingDuration(snapshot, fridayAfternoon, TimeSpan.FromHours(3))
            .ShouldBe(Utc(2026, 6, 15, 11));
    }

    [Fact]
    public void Should_Skip_Closed_Exception_And_Respect_Special_Hours()
    {
        var snapshot = CreateWeekdaySnapshot(
            new CalendarExceptionSnapshot(DateOnly.FromDateTime(new DateTime(2026, 6, 9)), CalendarExceptionKind.Closed, Array.Empty<WorkingHourRange>()),
            new CalendarExceptionSnapshot(
                DateOnly.FromDateTime(new DateTime(2026, 6, 10)),
                CalendarExceptionKind.SpecialHours,
                new[] { new WorkingHourRange(new TimeOnly(12, 0), new TimeOnly(14, 0)) }));

        _calculator.IsOpenAt(snapshot, Utc(2026, 6, 9, 10)).ShouldBeFalse();
        _calculator.IsOpenAt(snapshot, Utc(2026, 6, 10, 10)).ShouldBeFalse();
        _calculator.IsOpenAt(snapshot, Utc(2026, 6, 10, 12)).ShouldBeTrue();
        _calculator.ElapsedWorkingTime(snapshot, Utc(2026, 6, 8, 9), Utc(2026, 6, 11, 17))
            .ShouldBe(TimeSpan.FromHours(18));
    }

    [Fact]
    public void Should_Return_Open_And_Close_Boundaries()
    {
        var snapshot = CreateWeekdaySnapshot();

        _calculator.NextOpenAt(snapshot, Utc(2026, 6, 8, 8, 59)).ShouldBe(Utc(2026, 6, 8, 9));
        _calculator.NextOpenAt(snapshot, Utc(2026, 6, 8, 9)).ShouldBe(Utc(2026, 6, 8, 9));
        _calculator.NextCloseAt(snapshot, Utc(2026, 6, 8, 16, 59)).ShouldBe(Utc(2026, 6, 8, 17));
        _calculator.NextCloseAt(snapshot, Utc(2026, 6, 8, 17)).ShouldBe(Utc(2026, 6, 8, 17));
    }

    private static CalendarSnapshot CreateAlwaysOpenSnapshot()
    {
        return new CalendarSnapshot(
            Guid.NewGuid(),
            null,
            CalendarKind.Public,
            "UTC",
            true,
            Array.Empty<WorkingHourRuleSnapshot>(),
            Array.Empty<CalendarExceptionSnapshot>());
    }

    private static CalendarSnapshot CreateWeekdaySnapshot(params CalendarExceptionSnapshot[] exceptions)
    {
        return new CalendarSnapshot(
            Guid.NewGuid(),
            null,
            CalendarKind.Public,
            "UTC",
            false,
            new[]
            {
                new WorkingHourRuleSnapshot(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
                new WorkingHourRuleSnapshot(DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
                new WorkingHourRuleSnapshot(DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
                new WorkingHourRuleSnapshot(DayOfWeek.Thursday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
                new WorkingHourRuleSnapshot(DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(17, 0))
            },
            exceptions);
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute = 0)
    {
        return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }
}
