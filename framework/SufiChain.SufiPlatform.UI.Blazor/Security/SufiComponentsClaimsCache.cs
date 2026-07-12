using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SufiChain.SufiPlatform.UI.Blazor.Security;

/// <summary>
/// Caches the current user's claims from AuthenticationStateProvider.
/// This provides synchronous access to claims without async calls.
/// 
/// Usage: Register as scoped service and call InitializeAsync() at app startup.
/// The cache automatically updates when authentication state changes.
/// </summary>
public class SufiComponentsClaimsCache : IDisposable
{
    private readonly AuthenticationStateProvider? _authenticationStateProvider;
    private bool _disposed;

    /// <summary>
    /// The cached ClaimsPrincipal for the current user.
    /// </summary>
    public ClaimsPrincipal Principal { get; private set; } = new ClaimsPrincipal();

    /// <summary>
    /// Whether the cache has been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    public SufiComponentsClaimsCache(AuthenticationStateProvider? authenticationStateProvider = null)
    {
        _authenticationStateProvider = authenticationStateProvider;
        
        if (_authenticationStateProvider != null)
        {
            _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }
    }

    /// <summary>
    /// Initializes the cache by fetching the current authentication state.
    /// Should be called once during app initialization.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        if (_authenticationStateProvider != null)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            Principal = authState.User;
            IsInitialized = true;
        }
    }

    /// <summary>
    /// Gets a claim value by type, or null if not found.
    /// </summary>
    public string? GetClaimValue(string claimType)
    {
        return Principal.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Gets all claims of the specified type.
    /// </summary>
    public IEnumerable<Claim> GetClaims(string claimType)
    {
        return Principal.FindAll(claimType);
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    public bool IsInRole(string role) => Principal.IsInRole(role);

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        try
        {
            var authState = await task;
            Principal = authState.User;
        }
        catch
        {
            // Auth state task failed, keep previous principal
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_authenticationStateProvider != null)
        {
            _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}
