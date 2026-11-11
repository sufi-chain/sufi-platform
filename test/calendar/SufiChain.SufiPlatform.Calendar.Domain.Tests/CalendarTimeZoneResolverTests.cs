using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar;

public class CalendarTimeZoneResolverTests
{
    [Theory]
    [InlineData("Asia/Tehran")]
    [InlineData("Iran Standard Time")]
    public void Resolves_Iana_And_Windows_Ids(string id)
    {
        var zone = CalendarTimeZoneResolver.Resolve(id);
        zone.BaseUtcOffset.ShouldBe(System.TimeSpan.FromHours(3.5));
    }

    [Theory]
    [InlineData("Not/AZone")]
    [InlineData("")]
    [InlineData(null)]
    public void Unknown_Id_Falls_Back_Without_Throwing(string? id)
    {
        CalendarTimeZoneResolver.Resolve(id, "UTC").BaseUtcOffset.ShouldBe(System.TimeSpan.Zero);
    }

    [Fact]
    public void Unknown_Preferred_And_Fallback_Uses_Local()
    {
        Should.NotThrow(() => CalendarTimeZoneResolver.Resolve("Not/AZone", "Also/Bad")).ShouldNotBeNull();
    }
}

public class CalendarTimingTests
{
    [Theory]
    [InlineData("UTC", "Asia/Tehran", "UTC")]
    [InlineData("Asia/Tehran", null, "Asia/Tehran")]
    [InlineData("", "Asia/Tehran", "Asia/Tehran")]
    [InlineData("   ", "Iran Standard Time", "Iran Standard Time")]
    [InlineData("Unspecified", "UTC", "UTC")]
    [InlineData("Not/AZone", "Asia/Tehran", "Asia/Tehran")]
    [InlineData(null, "Unspecified", "UTC")]
    [InlineData(null, null, "UTC")]
    [InlineData("", "Not/AZone", "UTC")]
    public void Empty_Or_Unknown_Zone_Uses_The_Timing_Setting_Then_Utc(string? requested, string? setting, string expected)
    {
        var resolved = CalendarTiming.ResolveId(requested, setting);
        CalendarTimeZoneResolver.TryFind(resolved)!.Id.ShouldBe(
            CalendarTimeZoneResolver.TryFind(expected)!.Id);
    }
}