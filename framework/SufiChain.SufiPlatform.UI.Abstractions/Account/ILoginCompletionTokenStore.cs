namespace SufiChain.SufiPlatform.UI.Abstractions.Account;

/// <summary>
/// Stores a one-time token so that after successful login in an Interactive Server circuit,
/// the client can perform a full-page GET to an endpoint that consumes the token and sets the auth cookie.
/// </summary>
public interface ILoginCompletionTokenStore
{
    /// <summary>
    /// When true, the host has registered a real store and the token + complete-login flow is used.
    /// When false, the component should use HttpContext.Response.Redirect (e.g. static SSR or native POST).
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Creates a one-time token for the given user and return URL. Token should expire after a short time (e.g. 1 minute).
    /// </summary>
    Task<string> CreateAsync(Guid userId, string? returnUrl, bool rememberMe = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes the token and returns the user id and return URL, or null if invalid/expired. Token is single-use.
    /// </summary>
    Task<(Guid userId, string? returnUrl, bool rememberMe)?> ConsumeAsync(string token, CancellationToken cancellationToken = default);
}
