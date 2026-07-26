using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AbpFeatureDefinition = Volo.Abp.Features.FeatureDefinition;
using AbpFeatureGroupDefinition = Volo.Abp.Features.FeatureGroupDefinition;

namespace SufiChain.SufiPlatform.Features;

public interface IDynamicFeatureDefinitionStoreInMemoryCache
{
    string CacheStamp { get; set; }

    SemaphoreSlim SyncSemaphore { get; }

    DateTime? LastCheckTime { get; set; }

    Task FillAsync(
        List<FeatureGroupDefinitionRecord> featureGroupRecords,
        List<FeatureDefinitionRecord> featureRecords);

    AbpFeatureDefinition? GetFeatureOrNull(string name);

    IReadOnlyList<AbpFeatureDefinition> GetFeatures();

    IReadOnlyList<AbpFeatureGroupDefinition> GetGroups();
}
