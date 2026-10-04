using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.UI.Blazor.Server.Circuit;

/// <summary>
/// Copies the circuit <see cref="AuthenticationState"/> into <see cref="ICurrentPrincipalAccessor"/>.
/// Interactive Server renders have no <c>HttpContext</c>, so the HTTP principal accessor is anonymous.
/// Setting-group checks use that accessor and then drop every group.
/// The first Blazor Web root render runs in <c>OnCircuitOpenedAsync</c>, not in an inbound activity,
/// so the principal is held for the life of the circuit and applied again around each inbound activity.
/// </summary>
public class BlazorServerPrincipalCircuitHandler : CircuitHandler
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ICurrentPrincipalAccessor _principalAccessor;
    private readonly object _gate = new();
    private IDisposable? _circuitPrincipal;

    public BlazorServerPrincipalCircuitHandler(
        AuthenticationStateProvider authenticationStateProvider,
        ICurrentPrincipalAccessor principalAccessor)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _principalAccessor = principalAccessor;
    }

    /// <summary>
    /// Runs before other circuit handlers so permission checks in <c>OnCircuitOpenedAsync</c> see the user.
    /// Inbound dispatch wraps from last to first, so this order also makes the principal the outer scope.
    /// </summary>
    public override int Order => int.MinValue;

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        await ApplyAsync(_authenticationStateProvider.GetAuthenticationStateAsync());
    }

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            using (_principalAccessor.Change(state.User))
            {
                await next(context);
            }
        };
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        Replace(null);
        return Task.CompletedTask;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _ = ApplyAsync(task);
    }

    private async Task ApplyAsync(Task<AuthenticationState> task)
    {
        try
        {
            var state = await task;
            Replace(state.User);
        }
        catch (Exception)
        {
            // Keep the previous principal when the authentication task fails.
        }
    }

    private void Replace(ClaimsPrincipal? principal)
    {
        lock (_gate)
        {
            _circuitPrincipal?.Dispose();
            _circuitPrincipal = principal == null ? null : _principalAccessor.Change(principal);
        }
    }
}
