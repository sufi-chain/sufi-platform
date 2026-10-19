using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Execution;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#384 slice 5: per-agent scopes (interim rule: read-only or full; no level = the user's own
/// permissions; unmarked tools count as write). Effective access = user ∩ agent.
/// </summary>
public class AgentScopeTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Reader = Guid.Parse("4ead0000-0000-0000-0000-000000000001");
    private static readonly Guid Editor = Guid.Parse("ed170000-0000-0000-0000-000000000002");
    private const string ReadPermission = "Demo.Read";
    private const string WritePermission = "Demo.Write";

    [Fact]
    public async Task Full_scope_agent_used_by_a_read_only_user_cannot_write()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Reader, Principal(Reader)).WithAgentScope(AgentToolScope.Full);

        // The write tool is hidden; the agent's Full level adds nothing the user lacks.
        (await h.OfferedAsync(call)).ShouldBe(new[] { "MCP_demo_read", "MCP_demo_unmarked" }, ignoreOrder: true);
        var result = await h.Write.ExecuteAsync(new WorkspaceContext { CallContext = call }, new());
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.PermissionDenied);
    }

    [Fact]
    public async Task Read_only_agent_used_by_an_editor_cannot_write()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Editor, Principal(Editor)).WithAgentScope(AgentToolScope.ReadOnly);

        (await h.OfferedAsync(call)).ShouldBe(new[] { "MCP_demo_read" });
        (await h.Write.ExecuteAsync(new WorkspaceContext { CallContext = call }, new())).ErrorMessage
            .ShouldBe(AgentCallErrorCodes.AgentScopeReadOnly);
        (await h.Read.ExecuteAsync(new WorkspaceContext { CallContext = call }, new())).Success.ShouldBeTrue();
    }

    [Fact]
    public async Task Read_only_agent_cannot_call_unmarked_or_external_tools()
    {
        using var h = new Harness();
        var external = Substitute.For<IMCPTool>();
        external.Name.Returns("external.srv.ping");
        external.ParameterSchema.Returns("{}");
        h.Tools.Add(external);
        var call = AgentCallContext.ForUser(TenantA, Editor, Principal(Editor)).WithAgentScope(AgentToolScope.ReadOnly);

        (await h.OfferedAsync(call)).ShouldBe(new[] { "MCP_demo_read" });
        MCPToolOfferFilter.ScopeRefusal(h.Unmarked, call).ShouldBe(AgentCallErrorCodes.AgentScopeReadOnly);
        MCPToolOfferFilter.ScopeRefusal(external, call).ShouldBe(AgentCallErrorCodes.AgentScopeReadOnly);
        (await h.Unmarked.ExecuteAsync(new WorkspaceContext { CallContext = call }, new())).ErrorMessage
            .ShouldBe(AgentCallErrorCodes.AgentScopeReadOnly);
    }

    [Fact]
    public async Task Agent_without_a_level_acts_with_the_users_own_permissions()
    {
        using var h = new Harness();
        var editor = AgentCallContext.ForUser(TenantA, Editor, Principal(Editor));
        var reader = AgentCallContext.ForUser(TenantA, Reader, Principal(Reader));

        editor.AgentScope.ShouldBeNull();
        (await h.OfferedAsync(editor)).ShouldBe(new[] { "MCP_demo_read", "MCP_demo_write", "MCP_demo_unmarked" }, ignoreOrder: true);
        (await h.OfferedAsync(reader)).ShouldBe(new[] { "MCP_demo_read", "MCP_demo_unmarked" }, ignoreOrder: true);
        (await h.Write.ExecuteAsync(new WorkspaceContext { CallContext = editor }, new())).Success.ShouldBeTrue();
        (await h.OfferedAsync(editor.WithAgentScope(AgentToolScope.Full)))
            .ShouldBe(new[] { "MCP_demo_read", "MCP_demo_write", "MCP_demo_unmarked" }, ignoreOrder: true);
    }

    [Fact]
    public void Scope_keeps_the_bound_identity_and_only_explicit_read_only_tools_are_read_only()
    {
        var call = AgentCallContext.ForUser(TenantA, Editor, Principal(Editor), "s-1");
        var scoped = call.WithAgentScope(AgentToolScope.ReadOnly);
        scoped.UserId.ShouldBe(call.UserId);
        scoped.TenantId.ShouldBe(call.TenantId);
        scoped.SessionId.ShouldBe("s-1");
        scoped.Principal.ShouldBeSameAs(call.Principal);
        call.AgentScope.ShouldBeNull();

        var t = typeof(MarkedService);
        ReflectionToolDiscoveryService.IsReadOnly(t.GetMethod(nameof(MarkedService.Read))!).ShouldBeTrue();
        ReflectionToolDiscoveryService.IsReadOnly(t.GetMethod(nameof(MarkedService.Unmarked))!).ShouldBeFalse();
    }

    public sealed class MarkedService
    {
        [SufiAiMcpTool("m.read", "x", ReadOnly = true)]
        public Task<string> Read() => Task.FromResult("ok");

        [SufiAiMcpTool("m.unmarked", "x")]
        public Task<string> Unmarked() => Task.FromResult("ok");
    }

    public sealed class DemoService
    {
        public Task<string> Run() => Task.FromResult("ok");
    }

    private static ClaimsPrincipal Principal(Guid userId) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.TenantId, TenantA.ToString())
        }, "test"));

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _services;

        public Harness()
        {
            Permissions = Substitute.For<IPermissionChecker>();
            Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), ReadPermission).Returns(true);
            Permissions.IsGrantedAsync(
                    Arg.Is<ClaimsPrincipal?>(p => p != null && p.FindFirst(AbpClaimTypes.UserId)!.Value == Editor.ToString()),
                    WritePermission)
                .Returns(true);
            _services = new ServiceCollection()
                .AddSingleton<ICurrentTenantAccessor>(AsyncLocalCurrentTenantAccessor.Instance)
                .AddSingleton<ICurrentTenant, CurrentTenant>()
                .AddSingleton<ICurrentPrincipalAccessor, ThreadCurrentPrincipalAccessor>()
                .AddSingleton(Permissions)
                .AddScoped<DemoService>()
                .BuildServiceProvider();
            var run = typeof(DemoService).GetMethod(nameof(DemoService.Run))!;
            Read = new InternalMCPTool("demo.read", "Read", "{}", typeof(DemoService), run, _services, ReadPermission,
                isReadOnly: true);
            Write = new InternalMCPTool("demo.write", "Write", "{}", typeof(DemoService), run, _services, WritePermission);
            Unmarked = new InternalMCPTool("demo.unmarked", "Unmarked", "{}", typeof(DemoService), run, _services,
                requiresAuthentication: true);
            Tools = new List<IMCPTool> { Read, Write, Unmarked };
        }

        public IPermissionChecker Permissions { get; }
        public InternalMCPTool Read { get; }
        public InternalMCPTool Write { get; }
        public InternalMCPTool Unmarked { get; }
        public List<IMCPTool> Tools { get; }

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
}
