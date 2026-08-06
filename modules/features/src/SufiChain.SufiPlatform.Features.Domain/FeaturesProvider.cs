using System;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Features;
using Volo.Abp;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Features;

public abstract class FeaturesProvider : IFeaturesProvider
{
    public abstract string Name { get; }

    protected IFeaturesStore Store { get; }

    protected FeaturesProvider(IFeaturesStore store)
    {
        Store = store;
    }

    public virtual bool Compatible(string providerName)
    {
        return providerName == Name;
    }

    public virtual Task<IAsyncDisposable> HandleContextAsync(string providerName, string providerKey)
    {
        return Task.FromResult<IAsyncDisposable>(NullAsyncDisposable.Instance);
    }

    public virtual async Task<string> GetOrNullAsync(FeatureDefinition feature, string providerKey)
    {
        return await Store.GetOrNullAsync(feature.Name, Name, await NormalizeProviderKeyAsync(providerKey));
    }

    public virtual async Task SetAsync(FeatureDefinition feature, string value, string providerKey)
    {
        await Store.SetAsync(feature.Name, value, Name, await NormalizeProviderKeyAsync(providerKey));
    }

    public virtual async Task ClearAsync(FeatureDefinition feature, string providerKey)
    {
        await Store.DeleteAsync(feature.Name, Name, await NormalizeProviderKeyAsync(providerKey));
    }

    protected virtual Task<string> NormalizeProviderKeyAsync(string providerKey)
    {
        return Task.FromResult(providerKey);
    }
}
