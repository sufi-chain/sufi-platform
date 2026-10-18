using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Tenants;

public class SufiTenantValidator : ITenantValidator
{
    protected ITenantRepository TenantRepository { get; }

    public SufiTenantValidator(ITenantRepository tenantRepository)
    {
        TenantRepository = tenantRepository;
    }

    public virtual async Task ValidateAsync(Tenant tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.Name))
        {
            throw new ArgumentException("Tenant name can not be empty.", nameof(tenant));
        }

        if (string.IsNullOrWhiteSpace(tenant.NormalizedName))
        {
            throw new ArgumentException("Tenant normalized name can not be empty.", nameof(tenant));
        }

        var owner = await TenantRepository.FindByNameAsync(tenant.NormalizedName);
        if (owner != null && owner.Id != tenant.Id)
        {
            // sufi-chain#441: a coded, localized business error (was InvalidOperationException, shown as
            // the generic "internal error" modal).
            throw new BusinessException(TenantsErrorCodes.DuplicateTenantName)
                .WithData("Name", tenant.Name);
        }
    }
}
