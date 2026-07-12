using System.Security.Claims;

namespace SufiChain.SufiPlatform.UI.Users;

/// <summary>
/// Information about the current user.
/// </summary>
public class CurrentUserInfo
{
    /// <summary>
    /// Whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// The user's username.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The user's surname/last name.
    /// </summary>
    public string? SurName { get; set; }

    /// <summary>
    /// The user's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Whether the phone number is verified.
    /// </summary>
    public bool PhoneNumberVerified { get; set; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// The tenant ID if multi-tenancy is enabled.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The user's roles.
    /// </summary>
    public string[] Roles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// All claims associated with the user.
    /// </summary>
    public Claim[] Claims { get; set; } = Array.Empty<Claim>();
}
