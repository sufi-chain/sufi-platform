namespace SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

/// <summary>
/// Input DTO for adding users to an organization unit.
/// </summary>
public class OrganizationUnitUserInput
{
    /// <summary>
    /// The organization unit ID.
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    /// List of user IDs to add.
    /// </summary>
    public List<Guid> UserIds { get; set; } = new();
}
