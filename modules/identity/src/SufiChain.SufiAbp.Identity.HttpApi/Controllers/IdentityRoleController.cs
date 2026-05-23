using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using global::SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.Controllers;

[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/identity/roles")]
public class IdentityRoleController : SufiAbpControllerBase, IIdentityRoleAppService
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
    public virtual Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        return _roleAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        return _roleAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _roleAppService.DeleteAsync(id);
    }
}
