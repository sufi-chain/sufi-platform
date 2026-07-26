using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Identity.Integration;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Identity.Controllers.Integration;

/// <summary>
/// HTTP API for organization unit IntegrationService.
/// </summary>
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Area(IdentityRemoteServiceConsts.ModuleName)]
[ControllerName("OrganizationUnitIntegration")]
[Route("integration-api/identity/organization-units")]
public class OrganizationUnitIntegrationController : SufiControllerBase, IOrganizationUnitIntegrationService
{
    protected IOrganizationUnitIntegrationService OrganizationUnitIntegrationService { get; }

    public OrganizationUnitIntegrationController(IOrganizationUnitIntegrationService organizationUnitIntegrationService)
    {
        OrganizationUnitIntegrationService = organizationUnitIntegrationService;
    }

    [HttpGet]
    [Route("{organizationUnitId}/members/user-ids")]
    public virtual Task<List<Guid>> GetMemberUserIdsAsync(Guid organizationUnitId, bool includeChildren = false)
    {
        return OrganizationUnitIntegrationService.GetMemberUserIdsAsync(organizationUnitId, includeChildren);
    }
}
