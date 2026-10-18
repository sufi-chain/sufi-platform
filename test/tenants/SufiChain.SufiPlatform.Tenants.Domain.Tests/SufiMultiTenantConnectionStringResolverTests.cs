using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class SufiMultiTenantConnectionStringResolverTests
{
    private const string HostDefault = "Host=host-default";
    private const string HostHelpDesk = "Host=host-helpdesk";
    private const string HostKnowledgeBase = "Host=host-kb";
    private const string HostTicketing = "Host=host-ticketing";
    private const string HostLiveChat = "Host=host-livechat";
    private const string TenantDefault = "Host=tenant-default";

    [Theory]
    [InlineData("SufiHelpDesk")]
    [InlineData("SufiHelpDeskKnowledgeBase")]
    [InlineData("SufiHelpDeskTicketing")]
    [InlineData("SufiHelpDeskLiveChat")]
    [InlineData("SufiAI")]
    [InlineData("Default")]
    public async Task Tenant_With_Only_Default_Should_Resolve_Every_Name_To_Tenant_Database(string connectionName)
    {
        var tenantId = Guid.NewGuid();
        var resolver = CreateResolver(
            tenantId,
            tenantConnectionStrings: new ConnectionStrings
            {
                [ConnectionStrings.DefaultConnectionStringName] = TenantDefault
            });

        var resolved = await resolver.ResolveAsync(connectionName);

        resolved.ShouldBe(TenantDefault);
        resolved.ShouldNotBe(HostHelpDesk);
        resolved.ShouldNotBe(HostKnowledgeBase);
        resolved.ShouldNotBe(HostTicketing);
        resolved.ShouldNotBe(HostLiveChat);
        resolved.ShouldNotBe(HostDefault);
    }

    [Fact]
    public async Task Tenant_Explicit_Module_Row_Should_Win_Over_Default()
    {
        var tenantId = Guid.NewGuid();
        const string tenantKb = "Host=tenant-kb-override";
        var resolver = CreateResolver(
            tenantId,
            tenantConnectionStrings: new ConnectionStrings
            {
                [ConnectionStrings.DefaultConnectionStringName] = TenantDefault,
                ["SufiHelpDeskKnowledgeBase"] = tenantKb
            });

        (await resolver.ResolveAsync("SufiHelpDeskKnowledgeBase")).ShouldBe(tenantKb);
        (await resolver.ResolveAsync("SufiHelpDesk")).ShouldBe(TenantDefault);
    }

    [Fact]
    public async Task Host_Resolution_Should_Keep_Module_Specific_Strings()
    {
        var resolver = CreateResolver(tenantId: null, tenantConnectionStrings: null);

        (await resolver.ResolveAsync("SufiHelpDeskKnowledgeBase")).ShouldBe(HostKnowledgeBase);
        (await resolver.ResolveAsync("SufiHelpDesk")).ShouldBe(HostHelpDesk);
        (await resolver.ResolveAsync("SufiHelpDeskTicketing")).ShouldBe(HostTicketing);
        (await resolver.ResolveAsync("SufiHelpDeskLiveChat")).ShouldBe(HostLiveChat);
        (await resolver.ResolveAsync("Default")).ShouldBe(HostDefault);
    }

    [Theory]
    [InlineData("SufiHelpDeskKnowledgeBase", HostKnowledgeBase)]
    [InlineData("SufiHelpDesk", HostHelpDesk)]
    [InlineData("Default", HostDefault)]
    [InlineData(null, HostDefault)]
    public async Task Tenant_Without_Connection_Strings_Should_Use_Host_Resolution(string? connectionName, string expected)
    {
        // 8.3 behaviour: a tenant without its own database shares the host database (TenantId filter).
        var resolver = CreateResolver(Guid.NewGuid(), tenantConnectionStrings: new ConnectionStrings());

        (await resolver.ResolveAsync(connectionName)).ShouldBe(expected);
    }

    [Fact]
    public async Task Tenant_With_Only_Module_Row_Should_Use_Row_For_That_Name_And_Host_For_Others()
    {
        const string tenantKb = "Host=tenant-kb-only";
        var resolver = CreateResolver(
            Guid.NewGuid(),
            tenantConnectionStrings: new ConnectionStrings { ["SufiHelpDeskKnowledgeBase"] = tenantKb });

        (await resolver.ResolveAsync("SufiHelpDeskKnowledgeBase")).ShouldBe(tenantKb);
        (await resolver.ResolveAsync("SufiHelpDesk")).ShouldBe(HostHelpDesk);
        (await resolver.ResolveAsync("Default")).ShouldBe(HostDefault);
    }

    private static SufiMultiTenantConnectionStringResolver CreateResolver(
        Guid? tenantId,
        ConnectionStrings? tenantConnectionStrings)
    {
        var options = new AbpDbConnectionOptions();
        options.ConnectionStrings.Default = HostDefault;
        options.ConnectionStrings["SufiHelpDesk"] = HostHelpDesk;
        options.ConnectionStrings["SufiHelpDeskKnowledgeBase"] = HostKnowledgeBase;
        options.ConnectionStrings["SufiHelpDeskTicketing"] = HostTicketing;
        options.ConnectionStrings["SufiHelpDeskLiveChat"] = HostLiveChat;

        var monitor = Substitute.For<IOptionsMonitor<AbpDbConnectionOptions>>();
        monitor.CurrentValue.Returns(options);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);

        var tenantStore = Substitute.For<ITenantStore>();
        if (tenantId.HasValue)
        {
            var configuration = new TenantConfiguration(tenantId.Value, "touristpanel")
            {
                ConnectionStrings = tenantConnectionStrings ?? new ConnectionStrings()
            };
            tenantStore.FindAsync(tenantId.Value).Returns(configuration);
            tenantStore.Find(tenantId.Value).Returns(configuration);
        }

        var services = new ServiceCollection();
        services.AddSingleton(tenantStore);
        var serviceProvider = services.BuildServiceProvider();

        return new SufiMultiTenantConnectionStringResolver(monitor, currentTenant, serviceProvider);
    }
}
