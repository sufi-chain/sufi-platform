using System.Globalization;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;
using SufiChain.SufiPlatform.UI.Blazor.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Tests;

public class CalendarToolbarClockTests
{
    [Fact]
    public void Missing_calendar_zone_on_a_utc_host_shows_Tehran_with_the_zone_label()
    {
        var utc = new DateTimeOffset(2026, 11, 27, 17, 12, 0, TimeSpan.Zero);
        var text = CalendarToolbarClock.FormatNow(utc, null, null, CultureInfo.InvariantCulture);
        var tehran = UserTimeZoneDisplay.Resolve("Asia/Tehran");
        var local = TimeZoneInfo.ConvertTime(utc, tehran).DateTime;

        text.ShouldContain(local.ToString("HH:mm", CultureInfo.InvariantCulture));
        text.ShouldContain("(Asia/Tehran)");
        text.ShouldNotContain("UTC");
        CalendarToolbarClock.Resolve(null, null).Id.ShouldBe("Asia/Tehran");
    }
}
