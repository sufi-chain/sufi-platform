using Microsoft.AspNetCore.Components.Authorization;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Enters the Blazor circuit user before setting-group permission checks.
/// Those checks read <see cref="ICurrentPrincipalAccessor"/>.
/// On an interactive circuit that accessor is anonymous unless the circuit user is applied first.
/// </summary>
public static class SettingComponentAuthorizationScope
{
    public static async Task<IDisposable> EnterAsync(
        AuthenticationStateProvider authenticationStateProvider,
        ICurrentPrincipalAccessor principalAccessor)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return principalAccessor.Change(state.User);
    }
}
