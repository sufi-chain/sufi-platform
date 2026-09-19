using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity.Integration;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Identity.Permissions;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Identity-backed organization unit IntegrationService.
/// </summary>
public class OrganizationUnitIntegrationService : SufiApplicationService, IOrganizationUnitIntegrationService
{
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }

    public OrganizationUnitIntegrationService(IOrganizationUnitRepository organizationUnitRepository)
    {
        OrganizationUnitRepository = organizationUnitRepository;
    }

    public virtual async Task<List<Guid>> GetMemberUserIdsAsync(Guid organizationUnitId, bool includeChildren = false)
    {
        return await OrganizationUnitRepository.GetMemberIdsAsync(organizationUnitId, includeChildren);
    }

    [Authorize(IdentityPermissions.OrganizationUnits.Default)]
    public virtual async Task<List<OrganizationUnitDisplayNameDto>> GetDisplayNamesAsync(OrganizationUnitDisplayNamesInput input)
    {
        var ids = input.Ids.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0) return new List<OrganizationUnitDisplayNameDto>();
        var units = await OrganizationUnitRepository.GetListAsync(ids, includeDetails: false);
        return units.Select(unit => new OrganizationUnitDisplayNameDto
        {
            Id = unit.Id,
            DisplayName = unit.DisplayName
        }).ToList();
    }
}
