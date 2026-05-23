using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;

/// <summary>
/// Input DTO for getting members of an organization unit.
/// </summary>
public class GetOrganizationUnitMembersInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// The organization unit ID.
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    /// Optional filter for searching members by name, username, or email.
    /// </summary>
    public string? Filter { get; set; }
}
