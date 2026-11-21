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
        var provider = Substitute.For<WebResearchOptionsProvider>(
            Substitute.For<ISettingProvider>(),
            Substitute.For<ICurrentTenant>());
        provider.GetAsync().Returns(new WebResearchOptions { Token = "private-test-token", Endpoint = "https://search.internal" });
        var service = CreateService(provider, Substitute.For<ISettingManager>(), tenantId: null);
        var response = await service.GetAsync();
        Assert.True(response.HasStoredToken);
        Assert.DoesNotContain("private-test-token", JsonSerializer.Serialize(response));
    }

    [Fact]
    public async Task Endpoint_change_cannot_forward_inherited_credential()
    {
        var provider = Substitute.For<WebResearchOptionsProvider>(
            Substitute.For<ISettingProvider>(),
            Substitute.For<ICurrentTenant>());
        provider.GetAsync().Returns(new WebResearchOptions { Token = "private-test-token", Endpoint = "https://original.internal" });
        var manager = Substitute.For<ISettingManager>();
        var service = CreateService(provider, manager, tenantId: null);
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
        var provider = Substitute.For<WebResearchOptionsProvider>(
            Substitute.For<ISettingProvider>(),
            Substitute.For<ICurrentTenant>());
        var service = CreateService(provider, manager, tenantId: null);
        await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new() { Enabled = true, Endpoint = endpoint }));
        Assert.Empty(manager.ReceivedCalls());
    }

    [Fact]
    public async Task Missing_settings_use_disabled_bounded_defaults()
    {
        var current = Substitute.For<ICurrentTenant>();
        current.Change(null).Returns(Substitute.For<IDisposable>());
        var provider = new WebResearchOptionsProvider(Substitute.For<ISettingProvider>(), current);
        var options = await provider.GetAsync();
        Assert.False(options.Enabled);
        Assert.Equal(5, options.MaxSearchResults);
        Assert.Equal(3, options.MaxPagesToFetch);
        Assert.Equal(30_000, options.MaxTotalContextCharacters);
    }

    [Fact]
    public async Task Tenant_get_update_and_test_are_rejected_without_writing()
    {
        var tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var provider = Substitute.For<WebResearchOptionsProvider>(
            Substitute.For<ISettingProvider>(),
            Substitute.For<ICurrentTenant>());
        var manager = Substitute.For<ISettingManager>();
        var search = Substitute.For<IWebSearchService>();
        var service = CreateService(provider, manager, tenantId, search);

        var getError = await Assert.ThrowsAsync<BusinessException>(() => service.GetAsync());
        var updateError = await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new()
        {
            Enabled = true,
            Endpoint = "https://search.internal",
            AuthorizationBearerToken = "tenant-token"
        }));
        var testError = await Assert.ThrowsAsync<BusinessException>(() => service.TestAsync());

        Assert.Equal("AI:WebResearchHostOnly", getError.Code);
        Assert.Equal("AI:WebResearchHostOnly", updateError.Code);
        Assert.Equal("AI:WebResearchHostOnly", testError.Code);
        Assert.DoesNotContain("tenant-token", getError.Code + updateError.Code + testError.Code);
        Assert.Empty(manager.ReceivedCalls());
        Assert.Empty(search.ReceivedCalls());
    }

    [Fact]
    public async Task Host_update_writes_global_scope_only()
    {
        var provider = Substitute.For<WebResearchOptionsProvider>(
            Substitute.For<ISettingProvider>(),
            Substitute.For<ICurrentTenant>());
        provider.GetAsync().Returns(new WebResearchOptions
        {
            Enabled = false,
            Endpoint = "https://search.internal",
            Token = "host-token"
        });
        var manager = Substitute.For<ISettingManager>();
        var service = CreateService(provider, manager, tenantId: null);

        await service.UpdateAsync(new UpdateWebResearchSettingsInput
        {
            Enabled = true,
            Endpoint = "https://search.internal",
            AuthorizationBearerToken = "new-host-token",
            SafeSearch = 1,
            SearchTimeoutSeconds = 8,
            FetchTimeoutSeconds = 8,
            MaxSearchResults = 5,
            MaxPagesToFetch = 3,
            MaxPageBytes = 1_048_576,
            MaxExtractedCharactersPerPage = 12_000,
            MaxTotalContextCharacters = 30_000
        });

        await manager.Received().SetGlobalAsync(WebResearchSettings.Enabled, "True");
        await manager.Received().SetGlobalAsync(WebResearchSettings.Endpoint, "https://search.internal");
        await manager.Received().SetGlobalAsync(WebResearchSettings.Token, "new-host-token");
        await manager.DidNotReceive().SetForTenantOrGlobalAsync(
            Arg.Any<Guid?>(),
            Arg.Any<string>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task Options_provider_reads_under_host_scope_even_when_tenant_is_current()
    {
        var tenantId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(WebResearchSettings.Endpoint).Returns("https://host-search.internal");
        settings.GetOrNullAsync(WebResearchSettings.Token).Returns("host-only-token");
        settings.GetOrNullAsync(WebResearchSettings.Enabled).Returns("true");

        var current = Substitute.For<ICurrentTenant>();
        current.Id.Returns(tenantId);
        current.IsAvailable.Returns(true);
        current.Change(null).Returns(Substitute.For<IDisposable>());

        var options = await new WebResearchOptionsProvider(settings, current).GetAsync();

        Assert.Equal("https://host-search.internal", options.Endpoint);
        Assert.Equal("host-only-token", options.Token);
        current.Received().Change(null);
    }

    [Fact]
    public void Setting_definitions_are_global_only()
    {
        var context = new FakeSettingDefinitionContext();
        new WebResearchSettingDefinitionProvider().Define(context);
        Assert.All(context.Definitions, definition =>
        {
            Assert.Contains(GlobalSettingValueProvider.ProviderName, definition.Providers);
            Assert.DoesNotContain(TenantSettingValueProvider.ProviderName, definition.Providers);
        });
    }

    private static WebResearchSettingsAppService CreateService(
        WebResearchOptionsProvider provider,
        ISettingManager manager,
        Guid? tenantId,
        IWebSearchService? search = null)
    {
        var current = Substitute.For<ICurrentTenant>();
        current.Id.Returns(tenantId);
        current.IsAvailable.Returns(tenantId.HasValue);

        var services = new ServiceCollection();
        services.AddSingleton(current);
        var service = new WebResearchSettingsAppService(provider, manager, search ?? Substitute.For<IWebSearchService>());
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private sealed class FakeSettingDefinitionContext : ISettingDefinitionContext
    {
        public List<SettingDefinition> Definitions { get; } = [];

        public void Add(params SettingDefinition[] definitions) => Definitions.AddRange(definitions);

        public SettingDefinition? GetOrNull(string name) =>
            Definitions.Find(definition => definition.Name == name);

        public IReadOnlyList<SettingDefinition> GetAll() => Definitions;
    }
}
