using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Tenants.Controllers;

[Area(TenantsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = TenantsRemoteServiceConsts.RemoteServiceName)]
[Route("api/tenants/tenants")]
[Authorize(TenantsPermissions.Tenants.Default)]
public class TenantController : SufiControllerBase, ITenantAppService
{
    private readonly ITenantAppService _tenantAppService;

    public TenantController(ITenantAppService tenantAppService)
    {
        _tenantAppService = tenantAppService;
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize(TenantsPermissions.Tenants.Default)]
    public virtual Task<TenantDto> GetAsync(Guid id)
    {
        return _tenantAppService.GetAsync(id);
    }

    [HttpGet]
    [Authorize(TenantsPermissions.Tenants.Default)]
    public virtual Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input)
    {
        return _tenantAppService.GetListAsync(input);
    }

    [HttpPost]
    [Authorize(TenantsPermissions.Tenants.Create)]
    public virtual Task<TenantDto> CreateAsync(TenantCreateDto input)
    {
        return _tenantAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    [Authorize(TenantsPermissions.Tenants.Update)]
    public virtual Task<TenantDto> UpdateAsync(Guid id, TenantUpdateDto input)
    {
        return _tenantAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize(TenantsPermissions.Tenants.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return _tenantAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("{id}/default-connection-string")]
    [Authorize(TenantsPermissions.Tenants.ManageConnectionStrings)]
    public virtual Task<string> GetDefaultConnectionStringAsync(Guid id)
    {
        return _tenantAppService.GetDefaultConnectionStringAsync(id);
    }

    [HttpPut]
    [Route("{id}/default-connection-string")]
    [Authorize(TenantsPermissions.Tenants.ManageConnectionStrings)]
    public virtual Task UpdateDefaultConnectionStringAsync(Guid id, string defaultConnectionString)
    {
        return _tenantAppService.UpdateDefaultConnectionStringAsync(id, defaultConnectionString);
    }

    [HttpDelete]
    [Route("{id}/default-connection-string")]
    [Authorize(TenantsPermissions.Tenants.ManageConnectionStrings)]
    public virtual Task DeleteDefaultConnectionStringAsync(Guid id)
    {
        return _tenantAppService.DeleteDefaultConnectionStringAsync(id);
    }

    [HttpPost]
    [Route("{id}/activate")]
    [Authorize(TenantsPermissions.Tenants.Update)]
    public virtual Task ActivateAsync(Guid id)
    {
        return _tenantAppService.ActivateAsync(id);
    }

    [HttpPost]
    [Route("{id}/deactivate")]
    [Authorize(TenantsPermissions.Tenants.Update)]
    public virtual Task DeactivateAsync(Guid id)
    {
        return _tenantAppService.DeactivateAsync(id);
    }
}
