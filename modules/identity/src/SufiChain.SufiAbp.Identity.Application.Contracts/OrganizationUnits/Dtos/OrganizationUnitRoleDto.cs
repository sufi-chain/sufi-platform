namespace SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;

/// <summary>
/// DTO for displaying a role assigned to an organization unit.
/// </summary>
public class OrganizationUnitRoleDto
{
    /// <summary>
    /// The role's ID.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// The role's name.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}
