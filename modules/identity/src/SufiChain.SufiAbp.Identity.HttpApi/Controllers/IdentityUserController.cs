using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using global::SufiChain.SufiAbp.Identity;
using Volo.Abp;

namespace SufiChain.SufiAbp.Identity.Controllers;

[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/identity/users")]
public class IdentityUserController : SufiAbpControllerBase, IIdentityUserAppService
{
    private readonly IIdentityUserAppService _userAppService;

    public IdentityUserController(IIdentityUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<IdentityUserDto> GetAsync(Guid id)
    {
        return _userAppService.GetAsync(id);
    }

    [HttpGet]
    public virtual Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetIdentityUsersInput input)
    {
        return _userAppService.GetListAsync(input);
    }

    [HttpPost]
    public virtual Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        return _userAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        return _userAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _userAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("{id}/roles")]
    public virtual Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id)
    {
        return _userAppService.GetRolesAsync(id);
    }

    [HttpGet]
    [Route("assignable-roles")]
    public virtual Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync()
    {
        return _userAppService.GetAssignableRolesAsync();
    }

    [HttpPut]
    [Route("{id}/roles")]
    public virtual Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input)
    {
        return _userAppService.UpdateRolesAsync(id, input);
    }

    [HttpGet]
    [Route("by-username/{userName}")]
    public virtual Task<IdentityUserDto> FindByUsernameAsync(string userName)
    {
        return _userAppService.FindByUsernameAsync(userName);
    }

    [HttpGet]
    [Route("by-email/{email}")]
    public virtual Task<IdentityUserDto> FindByEmailAsync(string email)
    {
        return _userAppService.FindByEmailAsync(email);
    }
}
