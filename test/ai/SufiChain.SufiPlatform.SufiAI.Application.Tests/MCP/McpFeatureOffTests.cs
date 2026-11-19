using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using System.Reflection;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Application.MCP.Servers;
using SufiChain.SufiPlatform.SufiAI.Application.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP.Audit;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.Mcp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using Xunit;
using AbpFeatureChecker = Volo.Abp.Features.IFeatureChecker;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.MCP;

/// <summary>
/// SMK-16 / AGT-01: MCP administration stays refused when <c>SufiAI.MCP</c> is off,
/// including for a caller who would otherwise pass authorization.
/// </summary>
[DependsOn(typeof(SufiAIApplicationTestModule))]
public class McpFeatureDisabledTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDataProtection();
        context.Services.Replace(ServiceDescriptor.Transient<AbpFeatureChecker, McpOffFeatureChecker>());
    }
}

public class McpOffFeatureChecker : AbpFeatureChecker
{
    public Task<string> GetOrNullAsync(string name)
    {
        return Task.FromResult(name == SufiAIFeatures.MCP ? "false" : "true");
    }

    public Task<bool> IsEnabledAsync(string name)
    {
        return Task.FromResult(name != SufiAIFeatures.MCP);
    }

    public Task<Dictionary<string, bool>> IsEnabledAsync(string[] names)
    {
        return Task.FromResult(names.ToDictionary(item => item, item => item != SufiAIFeatures.MCP));
    }
}

public class McpFeatureAttributeTests
{
    [Fact]
    public void Mcp_admin_services_require_the_mcp_feature()
    {
        Required(typeof(MCPServerAppService)).ShouldContain(SufiAIFeatures.MCP);
        Required(typeof(MCPToolAppService)).ShouldContain(SufiAIFeatures.MCP);
        Required(typeof(McpToolCallAuditAppService)).ShouldContain(SufiAIFeatures.MCP);
        Required(typeof(WorkspaceReadinessMcpAppService)).ShouldContain(SufiAIFeatures.MCP);
        Required(typeof(SufiAIChatAppService)).ShouldContain(SufiAIFeatures.MCP);
        RequiresEveryListedFeature(typeof(MCPServerAppService));
        RequiresEveryListedFeature(typeof(MCPToolAppService));
        RequiresEveryListedFeature(typeof(McpToolCallAuditAppService));
        RequiresEveryListedFeature(typeof(WorkspaceReadinessMcpAppService));
        RequiresEveryListedFeature(typeof(SufiAIChatAppService));
    }

    private static string[] Required(Type service)
    {
        return service.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Cast<MemberInfo>()
            .Append(service)
            .SelectMany(member => member.GetCustomAttributes(inherit: true))
            .OfType<RequiresFeatureAttribute>()
            .SelectMany(attribute => attribute.Features)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// ABP treats a multi-feature <c>RequiresFeature</c> as OR unless <c>RequiresAll</c> is set.
    /// MCP must stay closed when <c>SufiAI.Enable</c> is still on.
    /// </summary>
    private static void RequiresEveryListedFeature(Type service)
    {
        var attributes = service.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Cast<MemberInfo>()
            .Append(service)
            .SelectMany(member => member.GetCustomAttributes(inherit: true))
            .OfType<RequiresFeatureAttribute>()
            .Where(attribute => attribute.Features.Contains(SufiAIFeatures.MCP, StringComparer.Ordinal))
            .ToList();

        attributes.ShouldNotBeEmpty();
        attributes.ShouldAllBe(attribute => attribute.RequiresAll);
    }
}

public class McpFeatureOffTests : SufiAITestBase<McpFeatureDisabledTestModule>
{
    [Fact]
    public async Task Server_list_throws_when_mcp_is_off()
    {
        await ExpectFeatureRefusal(() => GetRequiredService<IMCPServerAppService>().GetListAsync());
    }

    [Fact]
    public async Task Tool_catalog_throws_when_mcp_is_off()
    {
        await ExpectFeatureRefusal(() => GetRequiredService<IMCPToolAppService>().GetCatalogAsync());
    }

    [Fact]
    public async Task Audit_list_throws_when_mcp_is_off()
    {
        await ExpectFeatureRefusal(() =>
            GetRequiredService<IMcpToolCallAuditAppService>().GetListAsync(new GetMcpToolCallAuditLogsInput()));
    }

    [Fact]
    public async Task Chat_with_tools_throws_when_mcp_is_off()
    {
        await ExpectFeatureRefusal(() =>
            GetRequiredService<ISufiAIChatAppService>().SendMessageWithToolsAsync(new SufiAISendChatMessageInput
            {
                WorkspaceName = "unused",
                Message = "unused"
            }));
    }

    [Fact]
    public async Task Workspace_readiness_tool_throws_when_mcp_is_off()
    {
        await ExpectFeatureRefusal(() => GetRequiredService<WorkspaceReadinessMcpAppService>().ListWorkspacesAsync());
    }

    private static async Task ExpectFeatureRefusal(Func<Task> call)
    {
        var exception = await Should.ThrowAsync<AbpAuthorizationException>(call);
        exception.Code.ShouldNotBeNull();
        exception.Code.ShouldStartWith("Volo.Feature:");
    }
}
