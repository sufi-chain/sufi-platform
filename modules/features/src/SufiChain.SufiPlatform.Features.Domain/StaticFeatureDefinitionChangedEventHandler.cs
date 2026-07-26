using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Features;
using Volo.Abp.StaticDefinitions;
using Volo.Abp.Threading;
using AbpFeatureDefinition = Volo.Abp.Features.FeatureDefinition;
using AbpFeatureGroupDefinition = Volo.Abp.Features.FeatureGroupDefinition;

namespace SufiChain.SufiPlatform.Features;

public class StaticFeatureDefinitionChangedEventHandler :
    ILocalEventHandler<StaticFeatureDefinitionChangedEvent>,
    ITransientDependency
{
    protected IStaticDefinitionCache<AbpFeatureGroupDefinition, Dictionary<string, AbpFeatureGroupDefinition>> GroupCache { get; }
    protected IStaticDefinitionCache<AbpFeatureDefinition, Dictionary<string, AbpFeatureDefinition>> DefinitionCache { get; }
    protected FeatureDynamicInitializer FeatureDynamicInitializer { get; }
    protected ICancellationTokenProvider CancellationTokenProvider { get; }

    public StaticFeatureDefinitionChangedEventHandler(
        IStaticDefinitionCache<AbpFeatureGroupDefinition, Dictionary<string, AbpFeatureGroupDefinition>> groupCache,
        IStaticDefinitionCache<AbpFeatureDefinition, Dictionary<string, AbpFeatureDefinition>> definitionCache,
        FeatureDynamicInitializer featureDynamicInitializer,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        GroupCache = groupCache;
        DefinitionCache = definitionCache;
        FeatureDynamicInitializer = featureDynamicInitializer;
        CancellationTokenProvider = cancellationTokenProvider;
    }

    public virtual async Task HandleEventAsync(StaticFeatureDefinitionChangedEvent eventData)
    {
        await GroupCache.ClearAsync();
        await DefinitionCache.ClearAsync();
        await FeatureDynamicInitializer.InitializeAsync(false, CancellationTokenProvider.Token);
    }
}
