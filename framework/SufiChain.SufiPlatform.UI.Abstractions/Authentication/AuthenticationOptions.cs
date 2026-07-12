namespace SufiChain.SufiPlatform.UI.Authentication;

/// <summary>
/// Configuration options for authentication URLs and behavior.
/// </summary>
public class AuthenticationOptions
{
    /// <summary>
    /// Gets or sets the login URL.
    /// </summary>
    public string LoginUrl { get; set; } = "Account/Login";

    /// <summary>
    /// Gets or sets the logout URL.
    /// </summary>
    public string LogoutUrl { get; set; } = "Account/Logout";

    /// <summary>
    /// Gets or sets the access denied URL.
    /// </summary>
    public string AccessDeniedUrl { get; set; } = "Account/AccessDenied";

    /// <summary>
    /// Gets or sets whether to use relative URLs for redirects.
    /// </summary>
    public bool UseRelativeUrls { get; set; } = true;
}
