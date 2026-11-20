using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiAI.Web;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
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

    /// <summary>
    /// SET-35: Web research is intentionally tenant-overridable (Global + Tenant providers).
    /// A tenant save must call SetForTenantOrGlobalAsync with the tenant id, not the host/global scope.
    /// </summary>
    [Fact]
    public async Task Tenant_update_writes_tenant_scope_and_does_not_write_global()
    {
        var tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var provider = Substitute.For<WebResearchOptionsProvider>(Substitute.For<ISettingProvider>());
        provider.GetAsync().Returns(new WebResearchOptions
        {
            Enabled = false,
            Endpoint = "https://search.internal",
            Token = "host-token"
        });
        var manager = Substitute.For<ISettingManager>();
        var service = CreateService(provider, manager, tenantId);

        await service.UpdateAsync(new UpdateWebResearchSettingsInput
        {
            Enabled = true,
            Endpoint = "https://search.internal",
            AuthorizationBearerToken = "tenant-token",
            SafeSearch = 1,
            SearchTimeoutSeconds = 8,
            FetchTimeoutSeconds = 8,
            MaxSearchResults = 5,
            MaxPagesToFetch = 3,
            MaxPageBytes = 1_048_576,
            MaxExtractedCharactersPerPage = 12_000,
            MaxTotalContextCharacters = 30_000
        });

        await manager.Received().SetForTenantOrGlobalAsync(tenantId, WebResearchSettings.Enabled, "True");
        await manager.Received().SetForTenantOrGlobalAsync(tenantId, WebResearchSettings.Endpoint, "https://search.internal");
        await manager.Received().SetForTenantOrGlobalAsync(tenantId, WebResearchSettings.Token, "tenant-token");
        await manager.DidNotReceive().SetForTenantOrGlobalAsync(
            Arg.Is<Guid?>(id => id == null),
            Arg.Any<string>(),
            Arg.Any<string?>());
        await manager.DidNotReceive().SetGlobalAsync(Arg.Any<string>(), Arg.Any<string?>());
    }

    private static WebResearchSettingsAppService CreateService(
        WebResearchOptionsProvider provider,
        ISettingManager manager,
        Guid? tenantId)
    {
        var current = Substitute.For<ICurrentTenant>();
        current.Id.Returns(tenantId);
        current.IsAvailable.Returns(tenantId.HasValue);

        var services = new ServiceCollection();
        services.AddSingleton(current);
        var service = new WebResearchSettingsAppService(provider, manager, Substitute.For<IWebSearchService>());
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }
}
