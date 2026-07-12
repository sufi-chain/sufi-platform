namespace SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

/// <summary>
/// Input DTO for adding roles to an organization unit.
/// </summary>
public class OrganizationUnitRoleInput
{
    /// <summary>
    /// The organization unit ID.
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    /// List of role IDs to add.
    /// </summary>
    public List<Guid> RoleIds { get; set; } = new();
}
