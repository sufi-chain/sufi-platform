using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;

/// <summary>
/// Input DTO for creating a new organization unit.
/// </summary>
public class CreateOrganizationUnitDto
{
    /// <summary>
    /// Display name of the organization unit.
    /// </summary>
    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Parent organization unit ID. Null to create a root unit.
    /// </summary>
    public Guid? ParentId { get; set; }
}
