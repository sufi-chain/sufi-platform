namespace SufiChain.SufiPlatform.AspNetCore.Authentication;

/// <summary>
/// Abstraction for accessing authentication tokens in Blazor applications.
/// </summary>
public interface ISufiAccessTokenProvider
{
    /// <summary>
    /// Attempts to get an access token for the current user.
    /// </summary>
    /// <returns>The access token result containing the token if successful.</returns>
    ValueTask<SufiAccessTokenResult> RequestAccessTokenAsync();

    /// <summary>
    /// Attempts to get an access token with specific options.
    /// </summary>
    /// <param name="options">Options for the token request.</param>
    /// <returns>The access token result containing the token if successful.</returns>
    ValueTask<SufiAccessTokenResult> RequestAccessTokenAsync(SufiAccessTokenRequestOptions options);
}

/// <summary>
/// Options for requesting an access token.
/// </summary>
public class SufiAccessTokenRequestOptions
{
    /// <summary>
    /// The scopes to request.
    /// </summary>
    public IEnumerable<string>? Scopes { get; set; }

    /// <summary>
    /// The return URL after authentication.
    /// </summary>
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// Result of an access token request.
/// </summary>
public class SufiAccessTokenResult
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static SufiAccessTokenResult Success(SufiAccessToken token) => new()
    {
        Status = SufiAccessTokenResultStatus.Success,
        Token = token
    };

    /// <summary>
    /// Creates a result indicating the user needs to sign in.
    /// </summary>
    public static SufiAccessTokenResult RequiresRedirect(string redirectUrl) => new()
    {
        Status = SufiAccessTokenResultStatus.RequiresRedirect,
        RedirectUrl = redirectUrl
    };

    /// <summary>
    /// The status of the token request.
    /// </summary>
    public SufiAccessTokenResultStatus Status { get; set; }

    /// <summary>
    /// The access token if the request was successful.
    /// </summary>
    public SufiAccessToken? Token { get; set; }

    /// <summary>
    /// The redirect URL if authentication is required.
    /// </summary>
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// Attempts to get the token, returning true if successful.
    /// </summary>
    public bool TryGetToken(out SufiAccessToken? token)
    {
        token = Token;
        return Status == SufiAccessTokenResultStatus.Success && token != null;
    }
}

/// <summary>
/// Status of an access token request.
/// </summary>
public enum SufiAccessTokenResultStatus
{
    /// <summary>
    /// The token was successfully retrieved.
    /// </summary>
    Success,

    /// <summary>
    /// The user needs to authenticate.
    /// </summary>
    RequiresRedirect
}

/// <summary>
/// Represents an access token.
/// </summary>
public class SufiAccessToken
{
    /// <summary>
    /// The access token value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTimeOffset Expires { get; set; }

    /// <summary>
    /// The granted scopes.
    /// </summary>
    public IReadOnlyList<string> GrantedScopes { get; set; } = Array.Empty<string>();
}
