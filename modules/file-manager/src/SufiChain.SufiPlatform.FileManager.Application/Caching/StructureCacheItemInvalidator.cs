using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.Features;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.Caching;

/// <summary>
/// Invalidates the structure cache when any FileStructure is created, updated, or deleted.
/// </summary>
public class StructureCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<FileStructures.FileStructure>>,
    ITransientDependency
{
    private readonly IDistributedCache<StructureCacheItem> _cache;
    private readonly IFeatureChecker _featureChecker;
    private readonly ICurrentTenant _currentTenant;

    public StructureCacheItemInvalidator(
        IDistributedCache<StructureCacheItem> cache,
        IFeatureChecker featureChecker,
        ICurrentTenant currentTenant)
    {
        _cache = cache;
        _featureChecker = featureChecker;
        _currentTenant = currentTenant;
    }

    public async Task HandleEventAsync(EntityChangedEventData<FileStructures.FileStructure> eventData)
    {
        if (!await _featureChecker.IsEnabledAsync(SufiFileManagerFeatures.Enable) ||
            !await _featureChecker.IsEnabledAsync(SufiFileManagerFeatures.FileStructures))
        {
            return;
        }

        await _cache.RemoveAsync(
            StructureCacheItem.GetCacheKey(_currentTenant.Id),
            considerUow: true);
    }
}
