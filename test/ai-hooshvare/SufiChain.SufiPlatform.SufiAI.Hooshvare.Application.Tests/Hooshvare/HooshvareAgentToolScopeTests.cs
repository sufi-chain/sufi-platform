using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Execution;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Application.Tests.Hooshvare;

/// <summary>
/// sufi-chain#384 slice 5: per-agent tool scope integration tests.
/// </summary>
public class HooshvareAgentToolScopeTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid ReadOnlyUser = Guid.Parse("4ead0000-0000-0000-0000-000000000001");
    private static readonly Guid EditorUser = Guid.Parse("ed170000-0000-0000-0000-000000000002");
    private const string ReadPermission = "Demo.Read";
    private const string WritePermission = "Demo.Write";

    private static ClaimsPrincipal Principal(Guid userId) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.TenantId, TenantA.ToString())
        }, "test"));

    private sealed class TestService
    {
        [SufiAiMcpTool("demo.read", "Read", ReadOnly = true)]
        public Task<string> Read() => Task.FromResult("ok");

        [SufiAiMcpTool("demo.write", "Write")]
        public Task<string> Write() => Task.FromResult("ok");

        [SufiAiMcpTool("demo.unmarked", "Unmarked")]
        public Task<string> Unmarked() => Task.FromResult("ok");
    }

    private sealed class TestHarness : IDisposable
    {
        private readonly ServiceProvider _services;
        public IPermissionChecker Permissions { get; }
        public InternalMCPTool ReadTool { get; }
        public InternalMCPTool WriteTool { get; }
        public InternalMCPTool UnmarkedTool { get; }
        public List<IMCPTool> Tools { get; }

        public TestHarness()
        {
            Permissions = Substitute.For<IPermissionChecker>();
            Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), ReadPermission).Returns(true);
            Permissions.IsGrantedAsync(
                    Arg.Is<ClaimsPrincipal?>(p => p != null && p.FindFirst(AbpClaimTypes.UserId)!.Value == EditorUser.ToString()),
                    WritePermission)
                .Returns(true);

            _services = new ServiceCollection()
                .AddSingleton<ICurrentTenantAccessor>(AsyncLocalCurrentTenantAccessor.Instance)
                .AddSingleton<ICurrentTenant, CurrentTenant>()
                .AddSingleton<ICurrentPrincipalAccessor, ThreadCurrentPrincipalAccessor>()
                .AddSingleton(Permissions)
                .AddScoped<TestService>()
                .BuildServiceProvider();

            var run = typeof(TestService).GetMethod(nameof(TestService.Read))!;
            ReadTool = new InternalMCPTool("demo.read", "Read", "{}", typeof(TestService), run, _services, ReadPermission,
                isReadOnly: true);

            run = typeof(TestService).GetMethod(nameof(TestService.Write))!;
            WriteTool = new InternalMCPTool("demo.write", "Write", "{}", typeof(TestService), run, _services, WritePermission);

            run = typeof(TestService).GetMethod(nameof(TestService.Unmarked))!;
            UnmarkedTool = new InternalMCPTool("demo.unmarked", "Unmarked", "{}", typeof(TestService), run, _services,
                requiresAuthentication: true);

            Tools = new List<IMCPTool> { ReadTool, WriteTool, UnmarkedTool };
        }

        public async Task<string[]> OfferedAsync(AgentCallContext call)
        {
            var registry = Substitute.For<IMCPToolRegistry>();
            registry.ResolveAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
                .Returns(new MCPToolResolutionResult { Tools = Tools });

            var registrar = new MCPKernelToolRegistrar(
                registry,
                Array.Empty<IMcpToolDecisionGate>(),
                NullLogger<MCPKernelToolRegistrar>.Instance,
                serviceProvider: _services);

            var kernel = new Kernel();
            await registrar.RegisterToolsAsync(
                kernel,
                new WorkspaceContext { WorkspaceName = "demo", CallContext = call },
                Tools.Select(t => t.Name).ToList());

            return kernel.Plugins.Select(p => p.Name).ToArray();
        }

        public void Dispose() => _services.Dispose();
    }

    [Fact]
    public void A_hooshvare_without_a_level_acts_with_the_users_own_permissions()
    {
        var definition = Create();
        definition.GetAgentToolScope().ShouldBeNull();
        HooshvareAgentToolScopes.Resolve(definition.GetAgentToolScope()).ShouldBeNull();
    }

    [Theory]
    [InlineData("ReadOnly", HooshvareAgentToolScopeValues.ReadOnly)]
    [InlineData("readonly", HooshvareAgentToolScopeValues.ReadOnly)]
    [InlineData("Full", HooshvareAgentToolScopeValues.Full)]
    public void A_stored_level_is_applied(string value, string expected)
    {
        var definition = Create();
        definition.SetAgentToolScope(value);
        HooshvareAgentToolScopes.Resolve(definition.GetAgentToolScope()).ShouldBe(expected);
    }

    [Fact]
    public void Inherit_clears_the_level_and_unknown_values_are_rejected()
    {
        var definition = Create();
        definition.SetAgentToolScope("ReadOnly");
        definition.SetAgentToolScope(HooshvareDefinition.AgentToolScopeInherit);
        definition.GetAgentToolScope().ShouldBeNull();

        Should.Throw<Volo.Abp.BusinessException>(() => definition.SetAgentToolScope("Admin"));
    }

    [Fact]
    public void An_unreadable_stored_level_fails_closed_to_read_only()
    {
        var definition = Create();
        definition.SetProperty(HooshvareDefinition.AgentToolScopePropertyName, "garbage");
        HooshvareAgentToolScopes.Resolve(definition.GetAgentToolScope()).ShouldBe(HooshvareAgentToolScopeValues.ReadOnly);
        HooshvareAgentToolScopes.Resolve("7").ShouldBe(HooshvareAgentToolScopeValues.ReadOnly);
    }

    /// <summary>
    /// Test 1: A read-only agent never offers or runs a write tool.
    /// </summary>
    [Fact]
    public async Task ReadOnly_Agent_Never_Offers_Or_Runs_Write_Tool()
    {
        using var h = new TestHarness();

        var definition = Create();
        definition.SetAgentToolScope(HooshvareAgentToolScopeValues.ReadOnly);
        var agentScope = ToAgentScope(definition.GetAgentToolScope());
        agentScope.ShouldBe(AgentToolScope.ReadOnly);

        // The editor user has write permission, but the agent is ReadOnly
        var call = AgentCallContext.ForUser(TenantA, EditorUser, Principal(EditorUser)).WithAgentScope(agentScope);

        // Only read-only tools should be offered
        var offered = await h.OfferedAsync(call);
        offered.ShouldBe(new[] { "MCP_demo_read" }, ignoreOrder: true);

        // Write tool should be refused at offer time
        MCPToolOfferFilter.ScopeRefusal(h.WriteTool, call).ShouldBe(AgentCallErrorCodes.AgentScopeReadOnly);
        MCPToolOfferFilter.ScopeRefusal(h.UnmarkedTool, call).ShouldBe(AgentCallErrorCodes.AgentScopeReadOnly);

        // Write tool should fail at execution time
        var writeResult = await h.WriteTool.ExecuteAsync(new WorkspaceContext { CallContext = call }, new());
        writeResult.Success.ShouldBeFalse();
        writeResult.ErrorMessage.ShouldBe(AgentCallErrorCodes.AgentScopeReadOnly);

        // Read-only tool should still work
        var readResult = await h.ReadTool.ExecuteAsync(new WorkspaceContext { CallContext = call }, new());
        readResult.Success.ShouldBeTrue();
    }

    /// <summary>
    /// Test 2: A read-only user on a full-scope agent cannot run a write tool.
    /// </summary>
    [Fact]
    public async Task ReadOnly_User_On_FullScope_Agent_Cannot_Write()
    {
        using var h = new TestHarness();

        var definition = Create();
        definition.SetAgentToolScope(HooshvareAgentToolScopeValues.Full);
        var agentScope = ToAgentScope(definition.GetAgentToolScope());
        agentScope.ShouldBe(AgentToolScope.Full);

        // The read-only user does not have write permission
        var call = AgentCallContext.ForUser(TenantA, ReadOnlyUser, Principal(ReadOnlyUser)).WithAgentScope(agentScope);

        // Only tools the user has permission for should be offered (agent scope cannot add permissions)
        var offered = await h.OfferedAsync(call);
        offered.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_unmarked" }, ignoreOrder: true);

        // Write tool should fail because user lacks permission (even though agent has Full scope)
        var writeResult = await h.WriteTool.ExecuteAsync(new WorkspaceContext { CallContext = call }, new());
        writeResult.Success.ShouldBeFalse();
        writeResult.ErrorMessage.ShouldBe(AgentCallErrorCodes.PermissionDenied);
    }

    /// <summary>
    /// Test 3: Two users of the same agent get different tool lists from their own permissions.
    /// </summary>
    [Fact]
    public async Task Two_Users_Same_Agent_Get_Different_Tool_Lists()
    {
        using var h = new TestHarness();

        var definition = Create();
        definition.SetAgentToolScope(HooshvareAgentToolScopeValues.Full);
        var agentScope = ToAgentScope(definition.GetAgentToolScope());
        agentScope.ShouldBe(AgentToolScope.Full);

        // Editor user (has both read and write permissions)
        var editorCall = AgentCallContext.ForUser(TenantA, EditorUser, Principal(EditorUser)).WithAgentScope(agentScope);
        var editorOffered = await h.OfferedAsync(editorCall);
        editorOffered.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_write", "MCP_demo_unmarked" }, ignoreOrder: true);

        // Read-only user (only has read permission)
        var readerCall = AgentCallContext.ForUser(TenantA, ReadOnlyUser, Principal(ReadOnlyUser)).WithAgentScope(agentScope);
        var readerOffered = await h.OfferedAsync(readerCall);
        readerOffered.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_unmarked" }, ignoreOrder: true);

        // The two users get different tool lists based on their own permissions
        editorOffered.Length.ShouldBe(3);
        readerOffered.Length.ShouldBe(2);
        editorOffered.ShouldContain("MCP_demo_write");
        readerOffered.ShouldNotContain("MCP_demo_write");
    }

    /// <summary>
    /// Test 4: An empty scope falls back to the user's rights.
    /// </summary>
    [Fact]
    public async Task Empty_Scope_Falls_Back_To_User_Rights()
    {
        using var h = new TestHarness();

        var definition = Create();
        // No scope set - acts with user's own permissions
        definition.GetAgentToolScope().ShouldBeNull();
        HooshvareAgentToolScopes.Resolve(definition.GetAgentToolScope()).ShouldBeNull();
        HooshvareAgentToolScopes.Resolve((string?)null).ShouldBeNull();
        HooshvareAgentToolScopes.Resolve("").ShouldBeNull();

        var agentScope = ToAgentScope(definition.GetAgentToolScope());
        agentScope.ShouldBeNull();

        // Editor user gets all tools they have permission for
        var editorCall = AgentCallContext.ForUser(TenantA, EditorUser, Principal(EditorUser)).WithAgentScope(agentScope);
        var editorOffered = await h.OfferedAsync(editorCall);
        editorOffered.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_write", "MCP_demo_unmarked" }, ignoreOrder: true);

        // Read-only user gets only tools they have permission for
        var readerCall = AgentCallContext.ForUser(TenantA, ReadOnlyUser, Principal(ReadOnlyUser)).WithAgentScope(agentScope);
        var readerOffered = await h.OfferedAsync(readerCall);
        readerOffered.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_unmarked" }, ignoreOrder: true);

        // Agent scope is null, so it doesn't narrow or expand - just uses user's rights
        editorCall.AgentScope.ShouldBeNull();
        readerCall.AgentScope.ShouldBeNull();
    }

    /// <summary>
    /// Maps the Hooshvare's stored scope string to the tool-call enum, mirroring the Application layer.
    /// <c>null</c> means the agent inherits the user's own permissions.
    /// </summary>
    private static AgentToolScope? ToAgentScope(string? storedScope)
        => storedScope switch
        {
            HooshvareAgentToolScopeValues.ReadOnly => AgentToolScope.ReadOnly,
            HooshvareAgentToolScopeValues.Full => AgentToolScope.Full,
            _ => null
        };

    private static HooshvareDefinition Create() => (HooshvareDefinition)Activator.CreateInstance(typeof(HooshvareDefinition), nonPublic: true)!;
}