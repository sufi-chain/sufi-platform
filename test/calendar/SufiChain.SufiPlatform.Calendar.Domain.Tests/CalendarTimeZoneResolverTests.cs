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