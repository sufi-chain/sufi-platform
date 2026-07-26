using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Features;
using Volo.Abp.Threading;
using AbpFeatureDefinition = Volo.Abp.Features.FeatureDefinition;
using AbpFeatureGroupDefinition = Volo.Abp.Features.FeatureGroupDefinition;

namespace SufiChain.SufiPlatform.Features;

[Dependency(ReplaceServices = true)]
public class DynamicFeatureDefinitionStore : IDynamicFeatureDefinitionStore, ITransientDependency
{
    protected IFeatureGroupDefinitionRecordRepository FeatureGroupRepository { get; }
    protected IFeatureDefinitionRecordRepository FeatureRepository { get; }
    protected IFeatureDefinitionSerializer FeatureDefinitionSerializer { get; }
    protected IDynamicFeatureDefinitionStoreInMemoryCache StoreCache { get; }
    protected IDistributedCache DistributedCache { get; }
    protected IAbpDistributedLock DistributedLock { get; }
    public FeaturesOptions FeaturesOptions { get; }
    protected AbpDistributedCacheOptions CacheOptions { get; }

    public DynamicFeatureDefinitionStore(
        IFeatureGroupDefinitionRecordRepository featureGroupRepository,
        IFeatureDefinitionRecordRepository featureRepository,
        IFeatureDefinitionSerializer featureDefinitionSerializer,
        IDynamicFeatureDefinitionStoreInMemoryCache storeCache,
        IDistributedCache distributedCache,
        IOptions<AbpDistributedCacheOptions> cacheOptions,
        IOptions<FeaturesOptions> featuresOptions,
        IAbpDistributedLock distributedLock)
    {
        FeatureGroupRepository = featureGroupRepository;
        FeatureRepository = featureRepository;
        FeatureDefinitionSerializer = featureDefinitionSerializer;
        StoreCache = storeCache;
        DistributedCache = distributedCache;
        DistributedLock = distributedLock;
        FeaturesOptions = featuresOptions.Value;
        CacheOptions = cacheOptions.Value;
    }

    public virtual async Task<AbpFeatureDefinition?> GetOrNullAsync(string name)
    {
        if (!FeaturesOptions.IsDynamicFeatureStoreEnabled)
        {
            return null;
        }

        using (await StoreCache.SyncSemaphore.LockAsync())
        {
            await EnsureCacheIsUptoDateAsync();
            return StoreCache.GetFeatureOrNull(name);
        }
    }

    public virtual async Task<IReadOnlyList<AbpFeatureDefinition>> GetFeaturesAsync()
    {
        if (!FeaturesOptions.IsDynamicFeatureStoreEnabled)
        {
            return Array.Empty<AbpFeatureDefinition>();
        }

        using (await StoreCache.SyncSemaphore.LockAsync())
        {
            await EnsureCacheIsUptoDateAsync();
            return StoreCache.GetFeatures().ToImmutableList();
        }
    }

    public virtual async Task<IReadOnlyList<AbpFeatureGroupDefinition>> GetGroupsAsync()
    {
        if (!FeaturesOptions.IsDynamicFeatureStoreEnabled)
        {
            return Array.Empty<AbpFeatureGroupDefinition>();
        }

        using (await StoreCache.SyncSemaphore.LockAsync())
        {
            await EnsureCacheIsUptoDateAsync();
            return StoreCache.GetGroups().ToImmutableList();
        }
    }

    protected virtual async Task EnsureCacheIsUptoDateAsync()
    {
        if (StoreCache.LastCheckTime.HasValue &&
            DateTime.Now.Subtract(StoreCache.LastCheckTime.Value).TotalSeconds < 30)
        {
            return;
        }

        var stampInDistributedCache = await GetOrSetStampInDistributedCache();

        if (stampInDistributedCache == StoreCache.CacheStamp)
        {
            StoreCache.LastCheckTime = DateTime.Now;
            return;
        }

        await UpdateInMemoryStoreCache();

        StoreCache.CacheStamp = stampInDistributedCache;
        StoreCache.LastCheckTime = DateTime.Now;
    }

    protected virtual async Task UpdateInMemoryStoreCache()
    {
        var featureGroupRecords = await FeatureGroupRepository.GetListAsync();
        var featureRecords = await FeatureRepository.GetListAsync();

        await StoreCache.FillAsync(featureGroupRecords, featureRecords);
    }

    protected virtual async Task<string> GetOrSetStampInDistributedCache()
    {
        var cacheKey = GetCommonStampCacheKey();

        var stampInDistributedCache = await DistributedCache.GetStringAsync(cacheKey);
        if (stampInDistributedCache != null)
        {
            return stampInDistributedCache;
        }

        await using (var commonLockHandle = await DistributedLock
                         .TryAcquireAsync(GetCommonDistributedLockKey(), TimeSpan.FromMinutes(2)))
        {
            if (commonLockHandle == null)
            {
                throw new AbpException(
                    "Could not acquire distributed lock for feature definition common stamp check!"
                );
            }

            stampInDistributedCache = await DistributedCache.GetStringAsync(cacheKey);
            if (stampInDistributedCache != null)
            {
                return stampInDistributedCache;
            }

            stampInDistributedCache = Guid.NewGuid().ToString();

            await DistributedCache.SetStringAsync(
                cacheKey,
                stampInDistributedCache,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(30)
                }
            );
        }

        return stampInDistributedCache;
    }

    protected virtual string GetCommonStampCacheKey()
    {
        return $"{CacheOptions.KeyPrefix}_AbpInMemoryFeatureCacheStamp";
    }

    protected virtual string GetCommonDistributedLockKey()
    {
        return $"{CacheOptions.KeyPrefix}_Common_AbpFeatureUpdateLock";
    }
}
