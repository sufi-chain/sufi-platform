using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.TenantManagement.Tenants;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.TenantManagement.Controllers;

[Area(TenantManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = TenantManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/tenant-management/tenant-lookup")]
[AllowAnonymous]
public class TenantLookupController : SufiAbpControllerBase
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
