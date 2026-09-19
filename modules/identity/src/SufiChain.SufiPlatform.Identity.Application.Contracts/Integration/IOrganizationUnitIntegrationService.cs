using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Identity.Integration;

/// <summary>
/// Integration service for module-to-module organization unit member lookup.
/// </summary>
[IntegrationService]
public interface IOrganizationUnitIntegrationService : IApplicationService
{
    /// <summary>
    /// Returns user ids that are members of the given organization unit.
    /// </summary>
    /// <param name="organizationUnitId">Organization unit id.</param>
    /// <param name="includeChildren">When true, include members of child OUs (if supported by the store).</param>
    Task<List<Guid>> GetMemberUserIdsAsync(Guid organizationUnitId, bool includeChildren = false);

    /// <summary>Resolves organization unit labels in the current tenant with the normal OU read permission.</summary>
    Task<List<OrganizationUnitDisplayNameDto>> GetDisplayNamesAsync(OrganizationUnitDisplayNamesInput input);
}

public class OrganizationUnitDisplayNamesInput
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public List<Guid> Ids { get; set; } = new();
}

public class OrganizationUnitDisplayNameDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
