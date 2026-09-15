using System;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiAI.Web;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class WebResearchSettingsTests
{
    [Fact]
    public async Task Settings_response_never_contains_stored_secret()
    {
        var provider = Substitute.For<WebResearchOptionsProvider>(Substitute.For<ISettingProvider>());
        provider.GetAsync().Returns(new WebResearchOptions { Token = "private-test-token", Endpoint = "https://search.internal" });
        var service = new WebResearchSettingsAppService(provider, Substitute.For<ISettingManager>(), Substitute.For<IWebSearchService>());
        var response = await service.GetAsync();
        Assert.True(response.HasStoredToken);
        Assert.DoesNotContain("private-test-token", JsonSerializer.Serialize(response));
    }

    [Fact]
    public async Task Endpoint_change_cannot_forward_inherited_credential()
    {
        var provider = Substitute.For<WebResearchOptionsProvider>(Substitute.For<ISettingProvider>());
        provider.GetAsync().Returns(new WebResearchOptions { Token = "private-test-token", Endpoint = "https://original.internal" });
        var manager = Substitute.For<ISettingManager>();
        var service = new WebResearchSettingsAppService(provider, manager, Substitute.For<IWebSearchService>());
        var error = await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new() { Endpoint = "https://other.internal" }));
        Assert.Equal("AI:WebResearchTokenRequiredForEndpointChange", error.Code);
        Assert.Empty(manager.ReceivedCalls());
    }

    [Theory]
    [InlineData("http://search.internal")]
    [InlineData("https://user:password@search.internal")]
    [InlineData("https://search.internal?q=private")]
    public async Task Invalid_endpoint_is_rejected_before_any_setting_changes(string endpoint)
    {
        var manager = Substitute.For<ISettingManager>();
        var provider = Substitute.For<WebResearchOptionsProvider>(Substitute.For<ISettingProvider>());
        var service = new WebResearchSettingsAppService(provider, manager, Substitute.For<IWebSearchService>());
        await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new() { Enabled = true, Endpoint = endpoint }));
        Assert.Empty(manager.ReceivedCalls());
    }

    [Fact]
    public async Task Missing_settings_use_disabled_bounded_defaults()
    {
        var provider = new WebResearchOptionsProvider(Substitute.For<ISettingProvider>());
        var options = await provider.GetAsync();
        Assert.False(options.Enabled);
        Assert.Equal(5, options.MaxSearchResults);
        Assert.Equal(3, options.MaxPagesToFetch);
        Assert.Equal(30_000, options.MaxTotalContextCharacters);
    }
}
