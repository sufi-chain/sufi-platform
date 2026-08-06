using Volo.Abp;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Features;

public class TenantFeaturesProvider : FeaturesProvider, ITransientDependency
{
    public override string Name => TenantFeatureValueProvider.ProviderName;

    protected ICurrentTenant CurrentTenant { get; }

    public TenantFeaturesProvider(
        IFeaturesStore store,
        ICurrentTenant currentTenant)
        : base(store)
    {
        CurrentTenant = currentTenant;
    }

    public override Task<IAsyncDisposable> HandleContextAsync(string providerName, string providerKey)
    {
        if (providerName == Name && !providerKey.IsNullOrWhiteSpace())
        {
            if (Guid.TryParse(providerKey, out var tenantId))
            {
                var disposable = CurrentTenant.Change(tenantId);
                return Task.FromResult<IAsyncDisposable>(new AsyncDisposeFunc(() =>
                {
                    disposable.Dispose();
                    return Task.CompletedTask;
                }));
            }
        }

        return base.HandleContextAsync(providerName, providerKey);
    }

    protected override Task<string> NormalizeProviderKeyAsync(string providerKey)
    {
        if (providerKey != null)
        {
            return Task.FromResult(providerKey);
        }

        return Task.FromResult(CurrentTenant.Id?.ToString());
    }
}
