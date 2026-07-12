using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

/// <summary>
/// Input DTO for getting roles assigned to an organization unit.
/// </summary>
public class GetOrganizationUnitRolesInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// The organization unit ID.
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    /// Optional filter for searching roles by name.
    /// </summary>
    public string? Filter { get; set; }
}
