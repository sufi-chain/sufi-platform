using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.MCP;

public class McpAdminHostOnlyTests : SufiAITestBase<McpAdminHostOnlyTestModule>
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-0000-0000-0000-0000000000a7");

    [Fact]
    public async Task Tenant_admin_is_forbidden_from_mcp_admin_services()
    {
        using (GetRequiredService<ICurrentTenant>().Change(TenantA))
        {
            var servers = Should.Throw<AbpAuthorizationException>(() =>
                GetRequiredService<IMCPServerAppService>().GetListAsync().GetAwaiter().GetResult());
            var tools = Should.Throw<AbpAuthorizationException>(() =>
                GetRequiredService<IMCPToolAppService>().GetCatalogAsync().GetAwaiter().GetResult());

            servers.Code.ShouldStartWith("Volo.Authorization:");
            tools.Code.ShouldStartWith("Volo.Authorization:");
        }
    }

    [Fact]
    public async Task Host_admin_can_list_mcp_servers_and_tools()
    {
        var servers = await GetRequiredService<IMCPServerAppService>().GetListAsync();
        var tools = await GetRequiredService<IMCPToolAppService>().GetCatalogAsync();

        servers.ShouldNotBeNull();
        tools.ShouldNotBeNull();
    }
}

[DependsOn(typeof(SufiAIApplicationTestModule))]
public class McpAdminHostOnlyTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDataProtection();
        context.Services.Replace(ServiceDescriptor.Transient<IMethodInvocationAuthorizationService, McpSideMethodAuthorization>());
    }
}

/// <summary>
/// Applies the real MCP permission side. Tenant calls are 403; host calls proceed.
/// Other permissions stay allowed so tenant AI chat is not part of this gate.
/// </summary>
public sealed class McpSideMethodAuthorization : IMethodInvocationAuthorizationService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly PermissionDefinitionContext _permissions;

    public McpSideMethodAuthorization(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
        _permissions = new PermissionDefinitionContext(new ServiceCollection().BuildServiceProvider());
        new AIPermissionDefinitionProvider().Define(_permissions);
    }

    public Task CheckAsync(MethodInvocationAuthorizationContext context)
    {
        foreach (var policy in Policies(context.Method))
        {
            var permission = _permissions.GetPermissionOrNull(policy);
            if (permission == null)
            {
                continue;
            }

            if (!permission.MultiTenancySide.HasFlag(_currentTenant.GetMultiTenancySide()))
            {
                throw new AbpAuthorizationException(code: AbpAuthorizationErrorCodes.GivenPolicyHasNotGranted)
                    .WithData("PolicyName", policy);
            }
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<string> Policies(MethodInfo method)
    {
        return method.GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Concat(method.DeclaringType?.GetCustomAttributes<AuthorizeAttribute>(inherit: true) ?? [])
            .Select(attribute => attribute.Policy)
            .Where(policy => !string.IsNullOrWhiteSpace(policy))!;
    }
}
