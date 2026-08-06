using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

/// <summary>
/// Input DTO for updating an organization unit.
/// </summary>
public class UpdateOrganizationUnitDto
{
    /// <summary>
    /// New display name of the organization unit.
    /// </summary>
    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;
}
