using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SufiChain.Chat.Blazor.Public.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using Volo.Abp.Security.Claims;

namespace SufiChain.Chat.Blazor.Public;

/// <summary>
/// Base class for shared chat Blazor components used in messenger shells and widgets.
/// </summary>
public abstract class ChatPublicComponentBase : SufiAbpComponentBase
{
    private static readonly string[] UserIdClaimTypes =
    [
        AbpClaimTypes.UserId,
        ClaimTypes.NameIdentifier,
        "sub",
    ];

    /// <summary>
    /// Authenticated user id resolved from the Blazor authentication state.
    /// <see cref="SufiChain.SufiAbp.UI.Users.ICurrentUserAccessor"/> defaults to an anonymous stub in UI hosts.
    /// </summary>
    protected Guid? AuthenticatedUserId { get; private set; }

    /// <summary>
    /// Cascading parameter that provides the authentication state task.
    /// This is the proper way to access authentication state in Blazor Server
    /// without violating DI scope restrictions.
    /// </summary>
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    protected ChatPublicComponentBase()
    {
        LocalizationResource = typeof(ChatPublicResource);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await RefreshAuthenticatedUserIdAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        // Refresh user ID when cascading parameter changes
        await RefreshAuthenticatedUserIdAsync();
    }

    protected virtual async Task RefreshAuthenticatedUserIdAsync()
    {
        if (AuthenticationStateTask == null)
        {
            AuthenticatedUserId = null;
            return;
        }

        try
        {
            var authState = await AuthenticationStateTask;
            AuthenticatedUserId = ResolveAuthenticatedUserId(authState.User);
        }
        catch (InvalidOperationException)
        {
            // Handle cases where authentication state is not available yet
            AuthenticatedUserId = null;
        }
    }

    protected virtual Guid? ResolveAuthenticatedUserId(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        foreach (var claimType in UserIdClaimTypes)
        {
            var value = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var userId))
            {
                return userId;
            }
        }

        return null;
    }
}
