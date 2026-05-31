using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.FileManager.Features;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiAbp.FileManager.Caching;

/// <summary>
/// Invalidates the structure cache when any FileStructure is created, updated, or deleted.
/// </summary>
public class StructureCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<FileStructures.FileStructure>>,
    ITransientDependency
{
    private readonly IDistributedCache<StructureCacheItem> _cache;
    private readonly IFeatureChecker _featureChecker;

    public StructureCacheItemInvalidator(
        IDistributedCache<StructureCacheItem> cache,
        IFeatureChecker featureChecker)
    {
        _cache = cache;
        _featureChecker = featureChecker;
    }

    public async Task HandleEventAsync(EntityChangedEventData<FileStructures.FileStructure> eventData)
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAbpFileManagerFeatures.Enable) ||
            !await _featureChecker.IsEnabledAsync(SufiAbpFileManagerFeatures.FileStructures))
        {
            return;
        }

        await _cache.RemoveAsync(StructureCacheItem.CacheKey, considerUow: true);
    }
}
