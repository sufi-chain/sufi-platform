using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Scheduling;

public class CalendarMonthSlotTests
{
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    [Fact]
    public void CalendarView_MonthDayClick_OpensOneHourEvent()
    {
        var anotherDay = new DateTime(2026, 11, 27);
        var now = new DateTime(2026, 11, 28, 14, 10, 0);

        var other = CalendarMonthSlot.ForDay(anotherDay, now, Utc);
        other.StartUtc.ShouldBe(new DateTime(2026, 11, 27, 9, 0, 0, DateTimeKind.Utc));
        other.EndUtc.ShouldBe(other.StartUtc.AddHours(1));

        var today = CalendarMonthSlot.ForDay(now.Date, now, Utc);
        today.StartUtc.ShouldBe(new DateTime(2026, 11, 28, 15, 0, 0, DateTimeKind.Utc));
        today.EndUtc.ShouldBe(today.StartUtc.AddHours(1));
    }
}
