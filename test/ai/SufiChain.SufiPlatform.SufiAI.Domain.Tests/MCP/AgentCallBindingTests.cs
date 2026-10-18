using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#384 slice 1: an internal tool call runs only for the user and tenant bound in its
/// <see cref="AgentCallContext"/>, never as the ambient principal, another user or another tenant.
/// </summary>
public class AgentCallBindingTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid Alice = Guid.Parse("a11ce000-0000-0000-0000-000000000001");
    private static readonly Guid Mallory = Guid.Parse("3a110000-0000-0000-0000-000000000002");
    private const string Permission = "Demo.Write";

    [Fact]
    public async Task Call_without_context_is_refused_and_tool_never_runs()
    {
        using var h = new Harness();
        var result = await h.Tool().ExecuteAsync(new WorkspaceContext { TenantId = TenantA, UserId = Alice }, new());

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.CallContextMissing);
        h.Probe.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Tool_runs_as_bound_user_and_tenant_not_the_ambient_principal()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA), "session-1");

        // Ambient identity at execution time is someone else in another tenant.
        using (h.Tenant.Change(TenantB))
        using (h.Principals.Change(Principal(Mallory, TenantB)))
        {
            var result = await h.Tool().ExecuteAsync(new WorkspaceContext { CallContext = call }, new());
            result.Success.ShouldBeTrue(result.ErrorMessage);
        }

        h.Probe.SeenTenant.ShouldBe(TenantA);
        h.Probe.SeenUser.ShouldBe(Alice);
    }

    [Fact]
    public async Task Forged_tenant_in_workspace_context_is_refused()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA));

        var result = await h.Tool().ExecuteAsync(new WorkspaceContext { CallContext = call, TenantId = TenantB }, new());

        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.ForgedContext);
        h.Probe.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Forged_user_in_metadata_is_refused()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA));
        var workspace = new WorkspaceContext
        {
            CallContext = call,
            Metadata = new() { ["UserId"] = Mallory.ToString() }
        };

        var result = await h.Tool().ExecuteAsync(workspace, new());

        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.ForgedContext);
        h.Probe.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Principal_of_another_user_cannot_be_bound_to_alice()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Mallory, TenantA));

        var result = await h.Tool().ExecuteAsync(new WorkspaceContext { CallContext = call }, new());

        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.PrincipalMismatch);
        h.Probe.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Principal_from_another_tenant_is_refused()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantB));

        var result = await h.Tool().ExecuteAsync(new WorkspaceContext { CallContext = call }, new());

        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.TenantMismatch);
        h.Probe.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Declared_permission_is_checked_for_the_bound_user()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA));
        h.Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), Permission).Returns(false);

        var denied = await h.Tool(Permission).ExecuteAsync(new WorkspaceContext { CallContext = call }, new());
        denied.ErrorMessage.ShouldBe(AgentCallErrorCodes.PermissionDenied);
        h.Probe.Calls.ShouldBe(0);

        h.Permissions.IsGrantedAsync(
                Arg.Is<ClaimsPrincipal?>(p => p != null && p.FindFirst(AbpClaimTypes.UserId)!.Value == Alice.ToString()),
                Permission)
            .Returns(true);
        var allowed = await h.Tool(Permission).ExecuteAsync(new WorkspaceContext { CallContext = call }, new());
        allowed.Success.ShouldBeTrue(allowed.ErrorMessage);
        h.Probe.Calls.ShouldBe(1);
    }

    [Fact]
    public async Task Anonymous_turn_cannot_run_a_permissioned_tool()
    {
        using var h = new Harness();
        h.Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), Permission).Returns(true);

        var result = await h.Tool(Permission).ExecuteAsync(
            new WorkspaceContext { CallContext = AgentCallContext.Anonymous(TenantA) }, new());

        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.AnonymousNotAllowed);
        h.Probe.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task Anonymous_turn_runs_as_nobody_even_when_ambient_user_is_signed_in()
    {
        using var h = new Harness();
        using (h.Principals.Change(Principal(Mallory, TenantA)))
        {
            var result = await h.Tool().ExecuteAsync(
                new WorkspaceContext { CallContext = AgentCallContext.Anonymous(TenantA) }, new());
            result.Success.ShouldBeTrue(result.ErrorMessage);
        }

        h.Probe.SeenUser.ShouldBeNull();
        h.Probe.SeenTenant.ShouldBe(TenantA);
    }

    [Fact]
    public async Task Requires_permission_without_a_name_fails_closed()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA));

        var result = await h.Tool(permission: null, requiresPermission: true)
            .ExecuteAsync(new WorkspaceContext { CallContext = call }, new());

        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.PermissionNotDeclared);
        h.Probe.Calls.ShouldBe(0);
    }

    [Fact]
    public void Capture_without_user_claim_is_anonymous()
    {
        using var h = new Harness();
        using (h.Tenant.Change(TenantA))
        {
            var call = AgentCallContext.Capture(h.Tenant, h.Principals, "s");
            call.IsAnonymous.ShouldBeTrue();
            call.TenantId.ShouldBe(TenantA);
            call.SessionId.ShouldBe("s");
        }
    }

    [Fact]
    public void Capture_binds_signed_in_user_and_tenant()
    {
        using var h = new Harness();
        using (h.Tenant.Change(TenantA))
        using (h.Principals.Change(Principal(Alice, TenantA)))
        {
            var call = AgentCallContext.Capture(h.Tenant, h.Principals);
            call.UserId.ShouldBe(Alice);
            call.TenantId.ShouldBe(TenantA);
            AgentCallBinding.Validate(new WorkspaceContext { CallContext = call }).ShouldBeNull();
        }
    }

    private static ClaimsPrincipal Principal(Guid userId, Guid? tenantId)
    {
        var claims = new List<Claim> { new(AbpClaimTypes.UserId, userId.ToString()) };
        if (tenantId is not null)
        {
            claims.Add(new Claim(AbpClaimTypes.TenantId, tenantId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    public sealed class ProbeService
    {
        private readonly ICurrentTenant _tenant;
        private readonly ICurrentPrincipalAccessor _principals;
        private readonly ProbeLog _log;

        public ProbeService(ICurrentTenant tenant, ICurrentPrincipalAccessor principals, ProbeLog log)
        {
            _tenant = tenant;
            _principals = principals;
            _log = log;
        }

        public Task<string> RunAsync()
        {
            _log.Calls++;
            _log.SeenTenant = _tenant.Id;
            var user = _principals.Principal?.FindFirst(AbpClaimTypes.UserId)?.Value;
            _log.SeenUser = Guid.TryParse(user, out var id) ? id : null;
            return Task.FromResult("ok");
        }
    }

    public sealed class ProbeLog
    {
        public int Calls { get; set; }
        public Guid? SeenTenant { get; set; }
        public Guid? SeenUser { get; set; }
    }

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _services;

        public Harness()
        {
            Permissions = Substitute.For<IPermissionChecker>();
            Probe = new ProbeLog();
            _services = new ServiceCollection()
                .AddSingleton<ICurrentTenantAccessor>(AsyncLocalCurrentTenantAccessor.Instance)
                .AddSingleton<ICurrentTenant, CurrentTenant>()
                .AddSingleton<ICurrentPrincipalAccessor, ThreadCurrentPrincipalAccessor>()
                .AddSingleton(Permissions)
                .AddSingleton(Probe)
                .AddScoped<ProbeService>()
                .BuildServiceProvider();
            Tenant = _services.GetRequiredService<ICurrentTenant>();
            Principals = _services.GetRequiredService<ICurrentPrincipalAccessor>();
        }

        public IPermissionChecker Permissions { get; }
        public ProbeLog Probe { get; }
        public ICurrentTenant Tenant { get; }
        public ICurrentPrincipalAccessor Principals { get; }

        public InternalMCPTool Tool(string? permission = null, bool requiresPermission = false) =>
            new("demo.run", "Demo", "{}", typeof(ProbeService),
                typeof(ProbeService).GetMethod(nameof(ProbeService.RunAsync))!,
                _services, permission, requiresPermission);

        public void Dispose() => _services.Dispose();
    }
}
