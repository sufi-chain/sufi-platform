using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.Features;

public class EditionFeaturesProvider : FeaturesProvider, ITransientDependency
{
    public override string Name => EditionFeatureValueProvider.ProviderName;

    protected ICurrentPrincipalAccessor PrincipalAccessor { get; }
    protected ITenantStore TenantStore { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected string CurrentCompatibleProviderName { get; set; }

    public EditionFeaturesProvider(
        IFeaturesStore store,
        ICurrentPrincipalAccessor principalAccessor,
        ITenantStore tenantStore,
        ICurrentTenant currentTenant)
        : base(store)
    {
        PrincipalAccessor = principalAccessor;
        TenantStore = tenantStore;
        CurrentTenant = currentTenant;
    }

    public override bool Compatible(string providerName)
    {
        CurrentCompatibleProviderName = providerName;
        return providerName == TenantFeatureValueProvider.ProviderName || base.Compatible(providerName);
    }

    protected override async Task<string> NormalizeProviderKeyAsync(string providerKey)
    {
        return (await FindEditionIdAsync(providerKey))?.ToString();
    }

    protected virtual async Task<Guid?> FindEditionIdAsync(string providerKey)
    {
        if (Guid.TryParse(providerKey, out var parsedEditionOrTenantId))
        {
            if (CurrentCompatibleProviderName == TenantFeatureValueProvider.ProviderName)
            {
                var tenant = await TenantStore.FindAsync(parsedEditionOrTenantId);
                if (tenant != null)
                {
                    return tenant.EditionId;
                }
            }

            return parsedEditionOrTenantId;
        }

        if (CurrentTenant.Id.HasValue)
        {
            var tenant = await TenantStore.FindAsync(CurrentTenant.GetId());
            if (tenant != null)
            {
                return tenant.EditionId;
            }
        }
        //ToDo Editation module impilimenting
        var editionId = PrincipalAccessor.Principal?.FindFirst(AbpClaimTypes.EditionId)?.Value;
        return Guid.TryParse(editionId, out var parsedEditionId) ? parsedEditionId : null;
    }
}
