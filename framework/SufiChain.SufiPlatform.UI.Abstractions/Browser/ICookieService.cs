namespace SufiChain.SufiPlatform.UI.Browser;

/// <summary>
/// Service for managing browser cookies via JavaScript interop.
/// </summary>
public interface ICookieService
{
    /// <summary>
    /// Sets a cookie value.
    /// </summary>
    ValueTask SetAsync(string key, string value, CookieOptions? options = null);

    /// <summary>
    /// Gets a cookie value.
    /// </summary>
    ValueTask<string?> GetAsync(string key);

    /// <summary>
    /// Deletes a cookie.
    /// </summary>
    ValueTask DeleteAsync(string key, string? path = null);
}
