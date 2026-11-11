using System.Globalization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.UI.Blazor.Timing;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class UserTimeZoneDisplayTests
{
    private static readonly DateTime Utc = new(2026, 10, 30, 17, 55, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("Asia/Tehran", "07:15:59")]
    [InlineData("Asia/Tokyo", "12:45:59")]
    public void Stored_utc_audit_time_converts_with_the_saved_zone(string id, string expected)
    {
        var stored = new DateTime(2026, 6, 15, 3, 45, 59, DateTimeKind.Unspecified);

        UserTimeZoneDisplay.Format(stored, "HH:mm:ss", UserTimeZoneDisplay.Resolve(id), CultureInfo.InvariantCulture)
            .ShouldBe(expected);
    }

    [Theory]
    [InlineData("Asia/Tehran")]
    [InlineData("Iran Standard Time")]
    public void Configured_Tehran_ids_display_21_25(string id)
    {
        var text = UserTimeZoneDisplay.Format(Utc, "HH:mm", UserTimeZoneDisplay.Resolve(id), CultureInfo.InvariantCulture);
        text.ShouldBe("21:25");
        UserTimeZoneDisplay.Resolve(id).GetUtcOffset(Utc).ShouldBe(TimeSpan.FromMinutes(210));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Unspecified")]
    [InlineData("unspecified")]
    public void Unset_setting_defaults_to_Tehran_not_plus_three(string? id)
    {
        var zone = UserTimeZoneDisplay.Resolve(id);
        UserTimeZoneDisplay.Format(Utc, "HH:mm", zone, CultureInfo.InvariantCulture).ShouldBe("21:25");
        zone.GetUtcOffset(Utc).ShouldBe(TimeSpan.FromMinutes(210));
    }

    [Fact]
    public void Plus_three_is_not_the_Tehran_display()
    {
        var plus3 = TimeZoneInfo.CreateCustomTimeZone("UTC+3", TimeSpan.FromHours(3), "UTC+3", "UTC+3");
        UserTimeZoneDisplay.Format(Utc, "HH:mm", plus3, CultureInfo.InvariantCulture).ShouldBe("20:55");
        UserTimeZoneDisplay.Format(Utc, "HH:mm", UserTimeZoneDisplay.Resolve("Asia/Tehran"), CultureInfo.InvariantCulture).ShouldBe("21:25");
    }

    [Fact]
    public void Unspecified_kind_is_stored_utc()
    {
        var unspecified = new DateTime(2026, 10, 30, 17, 55, 0, DateTimeKind.Unspecified);
        UserTimeZoneDisplay.Format(unspecified, "yyyy-MM-dd HH:mm", UserTimeZoneDisplay.DefaultZone, CultureInfo.InvariantCulture)
            .ShouldBe("2026-10-30 21:25");
    }

    [Fact]
    public async Task Setting_provider_Asia_Tehran_displays_21_25()
    {
        var provider = Substitute.For<ISettingProvider>();
        provider.GetOrNullAsync(TimingSettingNames.TimeZone).Returns("Asia/Tehran");

        var zone = await new UserTimeZoneDisplay(provider).GetAsync();

        UserTimeZoneDisplay.Format(Utc, "HH:mm", zone, CultureInfo.InvariantCulture).ShouldBe("21:25");
    }

    [Fact]
    public async Task Missing_setting_uses_Tehran()
    {
        var provider = Substitute.For<ISettingProvider>();
        provider.GetOrNullAsync(TimingSettingNames.TimeZone).Returns((string?)null);

        var zone = await new UserTimeZoneDisplay(provider).GetAsync();

        UserTimeZoneDisplay.Format(Utc, "HH:mm", zone, CultureInfo.InvariantCulture).ShouldBe("21:25");
    }

    [Fact]
    public async Task Setting_read_failure_keeps_Tehran()
    {
        var provider = Substitute.For<ISettingProvider>();
        provider.GetOrNullAsync(TimingSettingNames.TimeZone).ThrowsAsync(new InvalidOperationException("settings down"));

        var zone = await new UserTimeZoneDisplay(provider).GetAsync();

        UserTimeZoneDisplay.Format(Utc, "HH:mm", zone, CultureInfo.InvariantCulture).ShouldBe("21:25");
    }
}
