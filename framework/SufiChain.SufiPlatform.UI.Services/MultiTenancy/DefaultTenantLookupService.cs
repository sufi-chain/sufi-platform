using SufiChain.SufiPlatform.UI.MultiTenancy;

namespace SufiChain.SufiPlatform.UI.Services.MultiTenancy;

/// <summary>
/// Default implementation of ITenantLookupService.
/// Returns empty list when tenant-management module is not loaded.
/// </summary>
public class DefaultTenantLookupService : ITenantLookupService
{
    public Task<TenantLookupResult> GetListAsync(string? filter, int skipCount, int maxResultCount)
    {
        return Task.FromResult(new TenantLookupResult
        {
            TotalCount = 0,
            Items = new List<TenantLookupItemDto>()
        });
    }
}
