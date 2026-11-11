using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests.Timing;

public class SufiUserTimeTests : BunitContext
{
    private static readonly DateTime StoredUtc = new(2026, 6, 15, 3, 45, 59, DateTimeKind.Unspecified);

    public SufiUserTimeTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData("Asia/Tehran", "2026-06-15 07:15:59")]
    [InlineData("Asia/Tokyo", "2026-06-15 12:45:59")]
    public void Audit_instant_renders_in_the_saved_zone(string id, string expected)
    {
        var display = Substitute.For<IUserTimeZoneDisplay>();
        display.GetAsync().Returns(UserTimeZoneDisplay.Resolve(id));
        Services.AddSingleton(display);

        var cut = Render<SufiUserTime>(parameters => parameters.Add(component => component.Value, StoredUtc));

        var time = cut.Find("bdi");
        time.TextContent.ShouldBe(expected);
        time.GetAttribute("dir").ShouldBe("ltr");
    }

    [Fact]
    public void Missing_instant_renders_the_empty_mark()
    {
        var display = Substitute.For<IUserTimeZoneDisplay>();
        Services.AddSingleton(display);

        var cut = Render<SufiUserTime>();

        cut.Find("bdi").TextContent.ShouldBe("-");
        display.DidNotReceive().GetAsync();
    }
}
