using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using SufiChain.SufiPlatform.SufiAI;
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
/// sufi-chain#384 slice 3: the model is only offered tools the bound caller may run.
/// </summary>
public class ToolOfferFilterTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Alice = Guid.Parse("a11ce000-0000-0000-0000-000000000001");
    private static readonly Guid Bob = Guid.Parse("b0b00000-0000-0000-0000-000000000003");
    private const string Write = "Demo.Write";

    [Fact]
    public async Task Two_users_on_the_same_agent_get_different_tool_lists()
    {
        using var h = new Harness();
        h.Permissions.IsGrantedAsync(
                Arg.Is<ClaimsPrincipal?>(p => p != null && p.FindFirst(AbpClaimTypes.UserId)!.Value == Alice.ToString()),
                Write)
            .Returns(true);

        var aliceTools = await h.OfferedAsync(AgentCallContext.ForUser(TenantA, Alice, Principal(Alice)));
        var bobTools = await h.OfferedAsync(AgentCallContext.ForUser(TenantA, Bob, Principal(Bob)));

        aliceTools.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_signedin", "MCP_demo_write", "MCP_demo_public" }, ignoreOrder: true);
        bobTools.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_signedin", "MCP_demo_public" }, ignoreOrder: true);
    }

    [Fact]
    public async Task Anonymous_turn_is_offered_only_allowlisted_tools()
    {
        using var h = new Harness();
        h.Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<string>()).Returns(true);
        var external = Substitute.For<IMCPTool>();
        external.Name.Returns("external.srv.ping");
        external.ParameterSchema.Returns("{}");
        h.Tools.Add(external);

        var offered = await h.OfferedAsync(AgentCallContext.Anonymous(TenantA));

        // slice 4: demo.read has no requirement but is not allowlisted; the external tool has no allowlist.
        offered.ShouldBe(new[] { "MCP_demo_public" });
    }

    [Fact]
    public async Task Forced_anonymous_call_to_an_unmarked_tool_is_refused()
    {
        using var h = new Harness();
        var anon = new WorkspaceContext { CallContext = AgentCallContext.Anonymous(TenantA) };

        (await h.Tools[0].ExecuteAsync(anon, new())).ErrorMessage.ShouldBe(AgentCallErrorCodes.AnonymousNotAllowed);
        (await h.Tools[3].ExecuteAsync(anon, new())).Success.ShouldBeTrue();

        var external = Substitute.For<IMCPTool>();
        MCPToolOfferFilter.AnonymousRefusal(external, anon.CallContext).ShouldBe(AgentCallErrorCodes.AnonymousNotAllowed);
        MCPToolOfferFilter.AnonymousRefusal(external, null).ShouldBe(AgentCallErrorCodes.AnonymousNotAllowed);
    }

    [Fact]
    public void Anonymous_allowlist_is_public_read_only_or_explicit_public_write()
    {
        var t = typeof(AllowlistService);
        ReflectionToolDiscoveryService.AllowsAnonymous(t.GetMethod(nameof(AllowlistService.PublicRead))!).ShouldBeTrue();
        ReflectionToolDiscoveryService.AllowsAnonymous(t.GetMethod(nameof(AllowlistService.ReadOnlyOnly))!).ShouldBeFalse();
        ReflectionToolDiscoveryService.AllowsAnonymous(t.GetMethod(nameof(AllowlistService.PublicWrite))!).ShouldBeTrue();
        ReflectionToolDiscoveryService.AllowsAnonymous(t.GetMethod(nameof(AllowlistService.PublicNotReadOnly))!).ShouldBeFalse();
    }

    public sealed class AllowlistService
    {
        [SufiAiMcpTool("a.public_read", "x", Public = true, ReadOnly = true)]
        public Task<string> PublicRead() => Task.FromResult("ok");

        [SufiAiMcpTool("a.read_only", "x", ReadOnly = true)]
        public Task<string> ReadOnlyOnly() => Task.FromResult("ok");

        [SufiAiMcpPublicWrite]
        [SufiAiMcpTool("a.public_write", "x")]
        public Task<string> PublicWrite() => Task.FromResult("ok");

        [SufiAiMcpTool("a.public_not_read_only", "x", Public = true)]
        public Task<string> PublicNotReadOnly() => Task.FromResult("ok");
    }

    [Fact]
    public async Task Forced_call_to_a_hidden_tool_is_still_refused_at_execution()
    {
        using var h = new Harness();
        h.Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<string>()).Returns(false);

        var bob = new WorkspaceContext { CallContext = AgentCallContext.ForUser(TenantA, Bob, Principal(Bob)) };
        (await h.Tools[2].ExecuteAsync(bob, new())).ErrorMessage.ShouldBe(AgentCallErrorCodes.PermissionDenied);

        var anon = new WorkspaceContext { CallContext = AgentCallContext.Anonymous(TenantA) };
        (await h.Tools[1].ExecuteAsync(anon, new())).ErrorMessage.ShouldBe(AgentCallErrorCodes.AnonymousNotAllowed);
    }

    [Fact]
    public void Authorize_metadata_is_folded_into_the_access_requirement()
    {
        var policyType = typeof(PolicyService);
        ReflectionToolDiscoveryService.GetAccessRequirement(policyType, policyType.GetMethod(nameof(PolicyService.Run))!, null)
            .ShouldBe(("Demo.Policy", true));
        ReflectionToolDiscoveryService.GetAccessRequirement(policyType, policyType.GetMethod(nameof(PolicyService.Run))!, "Demo.Explicit")
            .ShouldBe(("Demo.Explicit", true));
        ReflectionToolDiscoveryService.GetAccessRequirement(policyType, policyType.GetMethod(nameof(PolicyService.Public))!, null)
            .ShouldBe(((string?)null, false));

        var plainType = typeof(PlainService);
        ReflectionToolDiscoveryService.GetAccessRequirement(plainType, plainType.GetMethod(nameof(PlainService.Run))!, null)
            .ShouldBe(((string?)null, false));
        ReflectionToolDiscoveryService.GetAccessRequirement(plainType, plainType.GetMethod(nameof(PlainService.SignedIn))!, null)
            .ShouldBe(((string?)null, true));
    }

    private static ClaimsPrincipal Principal(Guid userId) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.TenantId, TenantA.ToString())
        }, "test"));

    [Authorize("Demo.Policy")]
    public sealed class PolicyService
    {
        public Task<string> Run() => Task.FromResult("ok");

        [AllowAnonymous]
        public Task<string> Public() => Task.FromResult("ok");
    }

    public sealed class PlainService
    {
        public Task<string> Run() => Task.FromResult("ok");

        [Authorize]
        public Task<string> SignedIn() => Task.FromResult("ok");
    }

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _services;

        public Harness()
        {
            Permissions = Substitute.For<IPermissionChecker>();
            _services = new ServiceCollection()
                .AddSingleton<ICurrentTenantAccessor>(AsyncLocalCurrentTenantAccessor.Instance)
                .AddSingleton<ICurrentTenant, CurrentTenant>()
                .AddSingleton<ICurrentPrincipalAccessor, ThreadCurrentPrincipalAccessor>()
                .AddSingleton(Permissions)
                .AddScoped<PlainService>()
                .BuildServiceProvider();
            var run = typeof(PlainService).GetMethod(nameof(PlainService.Run))!;
            Tools = new List<IMCPTool>
            {
                new InternalMCPTool("demo.read", "Read", "{}", typeof(PlainService), run, _services),
                new InternalMCPTool("demo.signedin", "Signed in", "{}", typeof(PlainService), run, _services,
                    requiresAuthentication: true),
                new InternalMCPTool("demo.write", "Write", "{}", typeof(PlainService), run, _services, Write),
                new InternalMCPTool("demo.public", "Public", "{}", typeof(PlainService), run, _services,
                    allowsAnonymous: true)
            };
        }

        public IPermissionChecker Permissions { get; }
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
