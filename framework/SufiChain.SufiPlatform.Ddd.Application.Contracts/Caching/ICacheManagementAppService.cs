using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Caching;

/// <summary>
/// Host backoffice contract for listing distributed caches and temporarily ignoring their values.
/// The management page is not built yet; this is the API it will call.
/// </summary>
public interface ICacheManagementAppService : IApplicationService
{
    Task<List<CacheManagementCacheDto>> GetListAsync();

    Task RevokeAsync(RevokeCacheInput input);

    Task LiftAsync(LiftCacheRevocationInput input);
}
