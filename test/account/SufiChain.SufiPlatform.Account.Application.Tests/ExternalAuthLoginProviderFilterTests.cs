using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class ExternalAuthLoginProviderFilterTests
{
    [Fact]
    public async Task Should_Hide_Google_When_Disabled_And_No_Host_Client_Id()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.ExternalAuth.Google.Enabled).Returns("false");
        settings.GetOrNullAsync(IdentitySettingNames.ExternalAuth.Google.ClientId).Returns("tenant-id");
        var configuration = Substitute.For<IConfiguration>();
        var filter = new ExternalAuthLoginProviderFilter(settings, configuration);

        var names = await filter.FilterEnabledAsync(["Google", "Facebook"]);

        names.ShouldBe(["Facebook"]);
    }

    [Fact]
    public async Task Should_Show_Google_When_Enabled_With_Client_Id()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.ExternalAuth.Google.Enabled).Returns("true");
        settings.GetOrNullAsync(IdentitySettingNames.ExternalAuth.Google.ClientId).Returns("tenant-id");
        var configuration = Substitute.For<IConfiguration>();
        var filter = new ExternalAuthLoginProviderFilter(settings, configuration);

        var names = await filter.FilterEnabledAsync(["Google"]);

        names.ShouldBe(["Google"]);
    }

    [Fact]
    public async Task Should_Show_Google_When_Host_Appsettings_Has_Client_Id()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.ExternalAuth.Google.Enabled).Returns("false");
        settings.GetOrNullAsync(IdentitySettingNames.ExternalAuth.Google.ClientId).Returns((string?)null);
        var configuration = Substitute.For<IConfiguration>();
        configuration["ExternalAuth:Google:ClientId"].Returns("host-id");
        var filter = new ExternalAuthLoginProviderFilter(settings, configuration);

        var names = await filter.FilterEnabledAsync(["Google"]);

        names.ShouldBe(["Google"]);
    }
}
