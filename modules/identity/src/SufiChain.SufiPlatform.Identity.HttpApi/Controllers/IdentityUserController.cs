using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using global::SufiChain.SufiPlatform.Identity;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Identity.Controllers;

[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Route("api/identity/users")]
public class IdentityUserController : SufiControllerBase, IIdentityUserAppService
{
    private readonly IIdentityUserAppService _userAppService;

    public IdentityUserController(IIdentityUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual Task<IdentityUserDto> GetAsync(Guid id)
    {
        return _userAppService.GetAsync(id);
    }

    [HttpGet]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetIdentityUsersInput input)
    {
        return _userAppService.GetListAsync(input);
    }

    [HttpPost]
    [Authorize(IdentityPermissions.Users.Create)]
    public virtual Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        return _userAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    [Authorize(IdentityPermissions.Users.Update)]
    public virtual Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        return _userAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize(IdentityPermissions.Users.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return _userAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("{id}/roles")]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id)
    {
        return _userAppService.GetRolesAsync(id);
    }

    [HttpGet]
    [Route("assignable-roles")]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync()
    {
        return _userAppService.GetAssignableRolesAsync();
    }

    [HttpPut]
    [Route("{id}/roles")]
    [Authorize(IdentityPermissions.Users.Update)]
    public virtual Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input)
    {
        return _userAppService.UpdateRolesAsync(id, input);
    }

    [HttpGet]
    [Route("by-username/{userName}")]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual Task<IdentityUserDto> FindByUsernameAsync(string userName)
    {
        return _userAppService.FindByUsernameAsync(userName);
    }

    [HttpGet]
    [Route("by-email/{email}")]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual Task<IdentityUserDto> FindByEmailAsync(string email)
    {
        return _userAppService.FindByEmailAsync(email);
    }
}
