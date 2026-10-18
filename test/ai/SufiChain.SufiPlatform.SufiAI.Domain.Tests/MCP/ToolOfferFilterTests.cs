using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

        aliceTools.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_signedin", "MCP_demo_write" }, ignoreOrder: true);
        bobTools.ShouldBe(new[] { "MCP_demo_read", "MCP_demo_signedin" }, ignoreOrder: true);
    }

    [Fact]
    public async Task Anonymous_turn_is_offered_only_tools_without_requirements()
    {
        using var h = new Harness();
        h.Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<string>()).Returns(true);

        var offered = await h.OfferedAsync(AgentCallContext.Anonymous(TenantA));

        offered.ShouldBe(new[] { "MCP_demo_read" });
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
                new InternalMCPTool("demo.write", "Write", "{}", typeof(PlainService), run, _services, Write)
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
