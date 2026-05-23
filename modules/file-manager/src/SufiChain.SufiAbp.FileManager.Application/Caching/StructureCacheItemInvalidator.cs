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

    public StructureCacheItemInvalidator(IDistributedCache<StructureCacheItem> cache)
    {
        _cache = cache;
    }

    public async Task HandleEventAsync(EntityChangedEventData<FileStructures.FileStructure> eventData)
    {
        await _cache.RemoveAsync(StructureCacheItem.CacheKey, considerUow: true);
    }
}
