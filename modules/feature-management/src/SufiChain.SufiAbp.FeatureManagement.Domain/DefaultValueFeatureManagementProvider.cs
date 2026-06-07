using Volo.Abp;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.FeatureManagement;

public class DefaultValueFeatureManagementProvider : IFeatureManagementProvider, ISingletonDependency
{
    public string Name => DefaultValueFeatureValueProvider.ProviderName;

    public bool Compatible(string providerName)
    {
        return providerName == Name;
    }

    public Task<IAsyncDisposable> HandleContextAsync(string providerName, string providerKey)
    {
        return Task.FromResult<IAsyncDisposable>(NullAsyncDisposable.Instance);
    }

    public virtual Task<string> GetOrNullAsync(FeatureDefinition feature, string providerKey)
    {
        return Task.FromResult(feature.DefaultValue);
    }

    public virtual Task SetAsync(FeatureDefinition feature, string value, string providerKey)
    {
        throw new AbpException($"Can not set default value of a feature. It is only possible while defining the feature in a {typeof(FeatureDefinitionProvider)} implementation.");
    }

    public virtual Task ClearAsync(FeatureDefinition feature, string providerKey)
    {
        throw new AbpException($"Can not clear default value of a feature. It is only possible while defining the feature in a {typeof(FeatureDefinitionProvider)} implementation.");
    }
}
