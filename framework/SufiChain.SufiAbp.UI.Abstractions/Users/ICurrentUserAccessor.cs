using System.Security.Claims;

namespace SufiChain.SufiAbp.UI.Users;

/// <summary>
/// Provides access to the current user information.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    Guid? Id { get; }

    /// <summary>
    /// The user's username.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// The user's surname/last name.
    /// </summary>
    string? SurName { get; }

    /// <summary>
    /// The user's phone number.
    /// </summary>
    string? PhoneNumber { get; }

    /// <summary>
    /// Whether the phone number is verified.
    /// </summary>
    bool PhoneNumberVerified { get; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Whether the email is verified.
    /// </summary>
    bool EmailVerified { get; }

    /// <summary>
    /// The tenant ID if multi-tenancy is enabled.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// The user's roles.
    /// </summary>
    string[] Roles { get; }

    /// <summary>
    /// Finds a claim by type.
    /// </summary>
    Claim? FindClaim(string claimType);

    /// <summary>
    /// Finds all claims with the given type.
    /// </summary>
    Claim[] FindClaims(string claimType);

    /// <summary>
    /// Gets all claims.
    /// </summary>
    Claim[] GetAllClaims();

    /// <summary>
    /// Checks if the user is in the specified role.
    /// </summary>
    bool IsInRole(string roleName);
}
