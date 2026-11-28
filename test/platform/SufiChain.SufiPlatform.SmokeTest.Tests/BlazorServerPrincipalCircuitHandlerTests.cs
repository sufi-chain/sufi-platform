using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Server.Circuit;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SmokeTest;

public class BlazorServerPrincipalCircuitHandlerTests
{
    [Fact]
    public async Task Opened_circuit_keeps_the_authenticated_user_until_the_circuit_closes()
    {
        var user = AuthenticatedUser();
        var accessor = new RecordingPrincipalAccessor();
        var handler = CreateHandler(user, accessor, new RecordingCurrentTenant());

        await handler.OnCircuitOpenedAsync(null!, CancellationToken.None);

        accessor.Principal.Identity!.IsAuthenticated.ShouldBeTrue();
        accessor.Principal.FindFirst(ClaimTypes.Name)!.Value.ShouldBe("admin");
        accessor.ActiveScopes.ShouldBe(1);

        await handler.OnCircuitClosedAsync(null!, CancellationToken.None);

        accessor.ActiveScopes.ShouldBe(0);
        accessor.Principal.Identity!.IsAuthenticated.ShouldBeFalse();
    }

    [Fact]
    public async Task Inbound_activity_sees_the_circuit_user_and_restores_the_previous_principal()
    {
        var user = AuthenticatedUser();
        var accessor = new RecordingPrincipalAccessor();
        var handler = CreateHandler(user, accessor, new RecordingCurrentTenant());
        var sawUser = false;

        var dispatch = handler.CreateInboundActivityHandler(_ =>
        {
            sawUser = accessor.Principal.Identity!.IsAuthenticated
                && accessor.Principal.FindFirst(ClaimTypes.Name)!.Value == "admin";
            return Task.CompletedTask;
        });

        await dispatch(CreateInboundContext());

        sawUser.ShouldBeTrue();
        accessor.ActiveScopes.ShouldBe(0);
        accessor.Principal.Identity!.IsAuthenticated.ShouldBeFalse();
    }

    [Fact]
    public void Handler_runs_before_other_circuit_handlers()
    {
        var handler = CreateHandler(AuthenticatedUser(), new RecordingPrincipalAccessor(), new RecordingCurrentTenant());

        handler.Order.ShouldBe(int.MinValue);
    }

    [Fact]
    public async Task UpdateModelConfiguration_FromCircuitEvent_KeepsTenant()
    {
        var tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var user = AuthenticatedUser(tenantId);
        var tenant = new RecordingCurrentTenant();
        var handler = CreateHandler(user, new RecordingPrincipalAccessor(), tenant);
        Guid? seen = Guid.NewGuid();

        var dispatch = handler.CreateInboundActivityHandler(_ =>
        {
            seen = tenant.Id;
            return Task.CompletedTask;
        });

        await dispatch(CreateInboundContext());

        seen.ShouldBe(tenantId);
        tenant.Id.ShouldBeNull();
    }

    private static BlazorServerPrincipalCircuitHandler CreateHandler(
        ClaimsPrincipal user,
        RecordingPrincipalAccessor accessor,
        RecordingCurrentTenant tenant)
    {
        return new BlazorServerPrincipalCircuitHandler(
            new FixedAuthenticationStateProvider(user),
            accessor,
            tenant);
    }

    private static CircuitInboundActivityContext CreateInboundContext()
    {
        var constructor = typeof(CircuitInboundActivityContext).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(Func<Task>), typeof(Circuit)],
            modifiers: null);
        return (CircuitInboundActivityContext)constructor!.Invoke([(Func<Task>)(() => Task.CompletedTask), null]);
    }

    private static ClaimsPrincipal AuthenticatedUser(Guid? tenantId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "admin") };
        if (tenantId.HasValue)
        {
            claims.Add(new Claim(AbpClaimTypes.TenantId, tenantId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }

    private sealed class RecordingCurrentTenant : ICurrentTenant
    {
        private readonly AsyncLocal<Guid?> _current = new();

        public bool IsAvailable => Id.HasValue;

        public Guid? Id => _current.Value;

        public string? Name => null;

        public IDisposable Change(Guid? id, string? name = null)
        {
            var parent = _current.Value;
            _current.Value = id;
            return new Scope(this, parent);
        }

        private sealed class Scope : IDisposable
        {
            private readonly RecordingCurrentTenant _tenant;
            private readonly Guid? _parent;

            public Scope(RecordingCurrentTenant tenant, Guid? parent)
            {
                _tenant = tenant;
                _parent = parent;
            }

            public void Dispose() => _tenant._current.Value = _parent;
        }
    }

    private sealed class FixedAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly Task<AuthenticationState> _state;

        public FixedAuthenticationStateProvider(ClaimsPrincipal user)
        {
            _state = Task.FromResult(new AuthenticationState(user));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => _state;
    }

    private sealed class RecordingPrincipalAccessor : ICurrentPrincipalAccessor
    {
        private readonly AsyncLocal<ClaimsPrincipal?> _current = new();

        public ClaimsPrincipal Principal =>
            _current.Value ?? new ClaimsPrincipal(new ClaimsIdentity());

        public int ActiveScopes { get; private set; }

        public IDisposable Change(ClaimsPrincipal principal)
        {
            var parent = _current.Value;
            _current.Value = principal;
            ActiveScopes++;
            return new Scope(this, parent);
        }

        private sealed class Scope : IDisposable
        {
            private readonly RecordingPrincipalAccessor _accessor;
            private readonly ClaimsPrincipal? _parent;
            private bool _disposed;

            public Scope(RecordingPrincipalAccessor accessor, ClaimsPrincipal? parent)
            {
                _accessor = accessor;
                _parent = parent;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _accessor._current.Value = _parent;
                _accessor.ActiveScopes--;
            }
        }
    }
}
