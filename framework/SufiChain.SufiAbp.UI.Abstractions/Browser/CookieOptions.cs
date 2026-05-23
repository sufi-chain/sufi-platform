namespace SufiChain.SufiAbp.UI.Browser;

/// <summary>
/// Options for setting browser cookies.
/// </summary>
public class CookieOptions
{
    /// <summary>
    /// Cookie expiration date/time.
    /// </summary>
    public DateTimeOffset? Expires { get; set; }

    /// <summary>
    /// Maximum age in seconds.
    /// </summary>
    public int? MaxAge { get; set; }

    /// <summary>
    /// Cookie path.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Cookie domain.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Whether the cookie should only be sent over HTTPS.
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// SameSite attribute value.
    /// </summary>
    public CookieSameSiteMode SameSite { get; set; } = CookieSameSiteMode.Lax;
}

/// <summary>
/// SameSite cookie attribute modes.
/// </summary>
public enum CookieSameSiteMode
{
    /// <summary>
    /// No SameSite attribute.
    /// </summary>
    None,

    /// <summary>
    /// Lax mode - cookies sent with top-level navigation.
    /// </summary>
    Lax,

    /// <summary>
    /// Strict mode - cookies only sent in first-party context.
    /// </summary>
    Strict
}
