using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Identity.Integration;
using SufiChain.SufiPlatform.Users;
using Volo.Abp;
using IdentityUserData = SufiChain.SufiPlatform.Identity.UserData;

namespace SufiChain.SufiPlatform.Identity.Controllers.Integration;

[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Area(IdentityRemoteServiceConsts.ModuleName)]
[ControllerName("UserIntegration")]
[Route("integration-api/identity/users")]
public class IdentityUserIntegrationController : SufiControllerBase, IIdentityUserIntegrationService
{
    protected IIdentityUserIntegrationService UserIntegrationService { get; }

    public IdentityUserIntegrationController(IIdentityUserIntegrationService userIntegrationService)
    {
        UserIntegrationService = userIntegrationService;
    }

    [HttpGet]
    [Route("{id}/role-names")]
    public virtual Task<string[]> GetRoleNamesAsync(Guid id)
    {
        return UserIntegrationService.GetRoleNamesAsync(id);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<IdentityUserData?> FindByIdAsync(Guid id)
    {
        return UserIntegrationService.FindByIdAsync(id);
    }

    [HttpGet]
    [Route("by-username/{userName}")]
    public virtual Task<IdentityUserData?> FindByUserNameAsync(string userName)
    {
        return UserIntegrationService.FindByUserNameAsync(userName);
    }

    [HttpGet]
    [Route("search")]
    public virtual Task<ListResultDto<IdentityUserData>> SearchAsync(UserLookupSearchInputDto input)
    {
        return UserIntegrationService.SearchAsync(input);
    }

    [HttpGet]
    [Route("search/by-ids")]
    public virtual Task<ListResultDto<IdentityUserData>> SearchByIdsAsync(Guid[] ids)
    {
        return UserIntegrationService.SearchByIdsAsync(ids);
    }

    [HttpGet]
    [Route("count")]
    public virtual Task<long> GetCountAsync(UserLookupCountInputDto input)
    {
        return UserIntegrationService.GetCountAsync(input);
    }

    [HttpGet]
    [Route("search/roles")]
    public virtual Task<ListResultDto<RoleData>> SearchRoleAsync(RoleLookupSearchInputDto input)
    {
        return UserIntegrationService.SearchRoleAsync(input);
    }

    [HttpGet]
    [Route("search/roles/by-names")]
    public virtual Task<ListResultDto<RoleData>> SearchRoleByNamesAsync(string[] names)
    {
        return UserIntegrationService.SearchRoleByNamesAsync(names);
    }

    [HttpGet]
    [Route("count/roles")]
    public virtual Task<long> GetRoleCountAsync(RoleLookupCountInputDto input)
    {
        return UserIntegrationService.GetRoleCountAsync(input);
    }
}
