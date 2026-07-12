using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Tenants.Tenants;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Tenants.Blazor.TenantSelector;

/// <summary>
/// Adapter that implements ITenantLookupService by delegating to ITenantLookupAppService.
/// Replaced by TenantsBlazorModule when tenant-management is loaded.
/// </summary>
[ExposeServices(typeof(ITenantLookupService))]
public class TenantLookupServiceAdapter : ITenantLookupService, ITransientDependency
{
    private readonly ITenantLookupAppService _tenantLookupAppService;

    public TenantLookupServiceAdapter(ITenantLookupAppService tenantLookupAppService)
    {
        _tenantLookupAppService = tenantLookupAppService;
    }

    public async Task<TenantLookupResult> GetListAsync(string? filter, int skipCount, int maxResultCount)
    {
        var input = new GetTenantLookupInput
        {
            Filter = filter,
            SkipCount = skipCount,
            MaxResultCount = maxResultCount
        };

        var result = await _tenantLookupAppService.GetListAsync(input);

        return new TenantLookupResult
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new SufiChain.SufiPlatform.UI.MultiTenancy.TenantLookupItemDto
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            }).ToList()
        };
    }
}
