using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SufiChain.SufiAbp.FileManager.Blazor.Services;

namespace SufiChain.SufiAbp.FileManager.Blazor.Server.Services;

/// <summary>
/// Server-side implementation of <see cref="IFileUploadAccessTokenProvider"/>.
/// Retrieves the access token from the HTTP context for use with direct HTTP uploads
/// that bypass SignalR in Blazor Server scenarios.
/// </summary>
public class ServerFileUploadAccessTokenProvider : IFileUploadAccessTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerFileUploadAccessTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the access token for the current user from the HTTP context.
    /// Returns null if no token is available (user not authenticated or token not saved).
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Get the access token from the authentication properties
        // This works because SaveTokens = true is set in OIDC configuration
        return await httpContext.GetTokenAsync("access_token");
    }

    /// <summary>
    /// Gets the access token or throws if not available.
    /// Use this when authentication is required.
    /// </summary>
    public async Task<string> GetRequiredAccessTokenAsync()
    {
        var token = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException(
                "Access token is not available. User may not be authenticated or tokens are not being saved.");
        }
        return token;
    }
}
