using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;

/// <summary>
/// Input DTO for moving an organization unit to a new parent.
/// </summary>
public class MoveOrganizationUnitDto
{
    /// <summary>
    /// ID of the organization unit to move.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// New parent ID. Null to make it a root unit.
    /// </summary>
    public Guid? NewParentId { get; set; }
}
