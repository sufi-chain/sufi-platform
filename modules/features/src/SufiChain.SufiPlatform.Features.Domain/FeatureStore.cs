using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Features;

public class FeatureStore : IFeatureStore, ITransientDependency
{
    protected IFeaturesStore FeaturesStore { get; }

    public FeatureStore(IFeaturesStore featureManagementStore)
    {
        FeaturesStore = featureManagementStore;
    }

    public virtual Task<string?> GetOrNullAsync(
        string name,
        string? providerName,
        string? providerKey)
    {
        return FeaturesStore.GetOrNullAsync(name, providerName, providerKey);
    }
}
