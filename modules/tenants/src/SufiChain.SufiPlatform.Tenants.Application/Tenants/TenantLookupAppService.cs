using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Tenants;

namespace SufiChain.SufiPlatform.Tenants.Tenants;

/// <summary>
/// Provides anonymous tenant lookup for the tenant selector on login/register pages.
/// Returns only Id and Name - no sensitive data.
/// </summary>
[AllowAnonymous]
public class TenantLookupAppService : TenantsAppServiceBase, ITenantLookupAppService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantLookupAppService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public virtual async Task<PagedResultDto<TenantLookupItemDto>> GetListAsync(GetTenantLookupInput input)
    {
        var sorting = input.Sorting ?? nameof(Tenant.Name);
        var count = await _tenantRepository.GetCountAsync(input.Filter);
        var list = await _tenantRepository.GetListAsync(
            sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.Filter
        );

        var items = list.Select(t => new TenantLookupItemDto
        {
            Id = t.Id,
            Name = t.Name ?? string.Empty
        }).ToList();

        return new PagedResultDto<TenantLookupItemDto>(count, items);
    }
}
