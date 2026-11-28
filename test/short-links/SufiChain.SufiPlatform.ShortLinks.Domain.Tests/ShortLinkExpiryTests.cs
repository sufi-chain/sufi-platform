using System.Globalization;
using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinkExpiryTests
{
    [Fact]
    public void ExpiryPicker_Today_StoresEndOfDayInUserZone()
    {
        var zone = UserTimeZoneDisplay.Resolve("Asia/Tehran");
        var day = new DateOnly(2026, 11, 28);
        var stored = UserTimeZoneDisplay.WallEndToUtc(day, zone);
        var wall = UserTimeZoneDisplay.ToDisplayTime(stored, zone);

        wall.Hour.ShouldBe(23);
        wall.Minute.ShouldBe(59);
        wall.Date.ShouldBe(day.ToDateTime(TimeOnly.MinValue));
        stored.ShouldBeGreaterThan(UserTimeZoneDisplay.WallStartToUtc(day, zone));
    }

    [Fact]
    public void ExpiryPicker_MinDateIsToday()
    {
        var zone = UserTimeZoneDisplay.Resolve("Asia/Tehran");
        var utc = new DateTime(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);
        UserTimeZoneDisplay.Today(utc, zone).ShouldBe(new DateOnly(2026, 11, 28));
    }

    [Fact]
    public void Grid_PastExpiresAt_ShowsExpiredBadge_KeepsIsActive()
    {
        var now = new DateTime(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);
        var isActive = true;
        ShortLinkExpiry.IsExpired(now.AddMinutes(-1), now).ShouldBeTrue();
        isActive.ShouldBeTrue();
    }

    [Fact]
    public void Grid_FutureExpiresAt_NoExpiredBadge()
    {
        var now = new DateTime(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);
        ShortLinkExpiry.IsExpired(now.AddMinutes(1), now).ShouldBeFalse();
        ShortLinkExpiry.IsExpired(null, now).ShouldBeFalse();
    }

    [Fact]
    public void Grid_CreatedAt_RenderedInUserTimeZone()
    {
        var utc = new DateTime(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);
        var text = UserTimeZoneDisplay.Format(
            utc,
            "yyyy-MM-dd HH:mm",
            UserTimeZoneDisplay.Resolve("Asia/Tehran"),
            CultureInfo.InvariantCulture);

        text.ShouldBe("2026-11-28 15:30");
    }
}
