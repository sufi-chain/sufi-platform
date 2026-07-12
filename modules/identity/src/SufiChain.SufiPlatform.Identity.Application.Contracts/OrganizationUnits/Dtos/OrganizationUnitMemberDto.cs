namespace SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

/// <summary>
/// DTO for displaying a member (user) of an organization unit.
/// </summary>
public class OrganizationUnitMemberDto
{
    /// <summary>
    /// The user's ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user's username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// The user's first name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string? Email { get; set; }
}
