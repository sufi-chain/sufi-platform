using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity.Integration;

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
}
