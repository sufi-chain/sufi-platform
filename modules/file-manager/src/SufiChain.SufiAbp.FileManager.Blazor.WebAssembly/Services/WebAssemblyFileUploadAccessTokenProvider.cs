using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SufiChain.SufiAbp.FileManager.Blazor.Services;

namespace SufiChain.SufiAbp.FileManager.Blazor.WebAssembly.Services;

/// <summary>
/// WebAssembly implementation of <see cref="IFileUploadAccessTokenProvider"/>.
/// Retrieves the access token using the WebAssembly authentication provider
/// for use with direct HTTP uploads from the browser.
/// </summary>
public class WebAssemblyFileUploadAccessTokenProvider : IFileUploadAccessTokenProvider
{
    private readonly IAccessTokenProvider _accessTokenProvider;

    public WebAssemblyFileUploadAccessTokenProvider(IAccessTokenProvider accessTokenProvider)
    {
        _accessTokenProvider = accessTokenProvider;
    }

    /// <summary>
    /// Gets the access token for the current user from the WebAssembly authentication provider.
    /// Returns null if no token is available (user not authenticated).
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        var tokenResult = await _accessTokenProvider.RequestAccessToken();
        
        if (tokenResult.TryGetToken(out var token))
        {
            return token.Value;
        }

        return null;
    }

    /// <summary>
    /// Gets the access token or throws if not available.
    /// Use this when authentication is required.
    /// </summary>
    public async Task<string> GetRequiredAccessTokenAsync()
    {
        var tokenResult = await _accessTokenProvider.RequestAccessToken();
        
        if (tokenResult.TryGetToken(out var token))
        {
            return token.Value;
        }

        // Check the status to provide a more helpful error message
        var status = tokenResult.Status;
        throw new UnauthorizedAccessException(
            $"Access token is not available. Token request status: {status}. " +
            "User may not be authenticated or token refresh failed.");
    }
}
