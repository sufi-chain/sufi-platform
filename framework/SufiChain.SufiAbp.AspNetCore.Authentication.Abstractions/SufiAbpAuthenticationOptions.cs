namespace SufiChain.SufiAbp.AspNetCore.Authentication;

/// <summary>
/// Options for SufiAbp authentication in Blazor applications.
/// Configures login/logout URLs for both Server and WebAssembly hosting models.
/// </summary>
public class SufiAbpAuthenticationOptions
{
    /// <summary>
    /// Login URL for Blazor Server hosting model.
    /// Server-side apps use MVC controllers to initiate OIDC challenges.
    /// Default: "Account/Login"
    /// </summary>
    public string ServerLoginUrl { get; set; } = "Account/Login";

    /// <summary>
    /// Logout URL for Blazor Server hosting model.
    /// Default: "Account/Logout"
    /// </summary>
    public string ServerLogoutUrl { get; set; } = "Account/Logout";

    /// <summary>
    /// Login URL for Blazor WebAssembly hosting model.
    /// WebAssembly apps use the authentication/login route.
    /// Default: "authentication/login"
    /// </summary>
    public string WebAssemblyLoginUrl { get; set; } = "authentication/login";

    /// <summary>
    /// Logout URL for Blazor WebAssembly hosting model.
    /// Default: "authentication/logout"
    /// </summary>
    public string WebAssemblyLogoutUrl { get; set; } = "authentication/logout";

    /// <summary>
    /// Whether this host acts as an OIDC client (tiered WebApp that redirects to a separate AuthServer).
    /// When true, Logout signs out of both <see cref="CookieScheme"/> and <see cref="OidcChallengeScheme"/>,
    /// and OidcLogin initiates an OIDC challenge.
    /// When false (default), the host is the AuthServer or a non-tiered all-in-one app;
    /// Logout signs out of ASP.NET Core Identity's ApplicationScheme cookie and redirects locally.
    /// Default: false
    /// </summary>
    public bool UseOidcClientFlow { get; set; } = false;

    /// <summary>
    /// The OIDC challenge scheme to use for server-side authentication.
    /// Only used when <see cref="UseOidcClientFlow"/> is true.
    /// Default: "oidc"
    /// </summary>
    public string OidcChallengeScheme { get; set; } = "oidc";

    /// <summary>
    /// The cookie authentication scheme.
    /// Only used when <see cref="UseOidcClientFlow"/> is true for OIDC client logout.
    /// Default: "Cookies"
    /// </summary>
    public string CookieScheme { get; set; } = "Cookies";
}
