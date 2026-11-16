using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using global::SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Identity.Controllers;

[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Authorize(IdentityPermissions.Roles.Default)]
[Route("api/identity/roles")]
public class IdentityRoleController : SufiControllerBase, IIdentityRoleAppService
{
    private readonly IIdentityRoleAppService _roleAppService;

    public IdentityRoleController(IIdentityRoleAppService roleAppService)
    {
        _roleAppService = roleAppService;
    }

    [HttpGet]
    [Route("all")]
    public virtual Task<ListResultDto<IdentityRoleDto>> GetAllListAsync()
    {
        return _roleAppService.GetAllListAsync();
    }

    [HttpGet]
    public virtual Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRolesInput input)
    {
        return _roleAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<IdentityRoleDto> GetAsync(Guid id)
    {
        return _roleAppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(IdentityPermissions.Roles.Create)]
    public virtual Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        return _roleAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    [Authorize(IdentityPermissions.Roles.Update)]
    public virtual Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        return _roleAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize(IdentityPermissions.Roles.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return _roleAppService.DeleteAsync(id);
    }
}
