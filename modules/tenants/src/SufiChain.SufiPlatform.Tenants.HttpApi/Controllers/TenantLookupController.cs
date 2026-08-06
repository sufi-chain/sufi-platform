using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Tenants.Tenants;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.Tenants.Controllers;

[Area(TenantsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = TenantsRemoteServiceConsts.RemoteServiceName)]
[Route("api/tenants/tenant-lookup")]
[AllowAnonymous]
public class TenantLookupController : SufiControllerBase
{
    private readonly ITenantLookupAppService _tenantLookupAppService;

    public TenantLookupController(ITenantLookupAppService tenantLookupAppService)
    {
        _tenantLookupAppService = tenantLookupAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<TenantLookupItemDto>> GetListAsync([FromQuery] GetTenantLookupInput input)
    {
        return _tenantLookupAppService.GetListAsync(input);
    }
}
