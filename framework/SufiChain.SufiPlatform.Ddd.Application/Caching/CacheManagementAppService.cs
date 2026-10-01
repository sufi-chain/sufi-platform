using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Localization.Resources.SufiDdd;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Caching;

[Authorize(CacheManagementPermissions.Manage)]
public class CacheManagementAppService : SufiApplicationService, ICacheManagementAppService
{
    private readonly ICacheCatalog _catalog;
    private readonly ICacheRevocationStore _revocations;

    public CacheManagementAppService(ICacheCatalog catalog, ICacheRevocationStore revocations)
    {
        _catalog = catalog;
        _revocations = revocations;
        LocalizationResource = typeof(SufiDddApplicationContractsResource);
    }

    public async Task<List<CacheManagementCacheDto>> GetListAsync()
    {
        var active = await _revocations.GetActiveAsync();
        return _catalog.GetAll()
            .Select(item => new CacheManagementCacheDto
            {
                CacheName = item.CacheName,
                ItemType = item.ItemType,
                Revocations = active
                    .Where(entry => string.Equals(entry.CacheName, item.CacheName, StringComparison.Ordinal))
                    .Select(entry => new CacheRevocationDto
                    {
                        Key = entry.Key,
                        RevokedUntilUtc = entry.RevokedUntilUtc
                    })
                    .OrderBy(entry => entry.Key ?? string.Empty, StringComparer.Ordinal)
                    .ToList()
            })
            .ToList();
    }

    public async Task RevokeAsync(RevokeCacheInput input)
    {
        EnsureKnownCache(input.CacheName);
        var until = DateTime.UtcNow.AddMinutes(input.DurationMinutes);
        await _revocations.RevokeAsync(input.CacheName, input.Key, until);
    }

    public async Task LiftAsync(LiftCacheRevocationInput input)
    {
        EnsureKnownCache(input.CacheName);
        await _revocations.LiftAsync(input.CacheName, input.Key);
    }

    private void EnsureKnownCache(string cacheName)
    {
        if (!_catalog.Contains(cacheName))
        {
            throw new UserFriendlyException(L["CacheManagement:UnknownCache", cacheName]);
        }
    }
}
