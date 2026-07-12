using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

/// <summary>
/// DTO for displaying an organization unit in a tree structure.
/// </summary>
public class OrganizationUnitDto : EntityDto<Guid>
{
    /// <summary>
    /// Parent organization unit ID. Null for root units.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Hierarchical code (e.g., "00001.00001.00001").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the organization unit.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Number of users in this organization unit.
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Number of roles assigned to this organization unit.
    /// </summary>
    public int RoleCount { get; set; }

    /// <summary>
    /// Child organization units.
    /// </summary>
    public List<OrganizationUnitDto> Children { get; set; } = new();
}
