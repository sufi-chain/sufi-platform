using System;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.TenantManagement;

public class SufiAbpTenantValidator : ITenantValidator
{
    protected ITenantRepository TenantRepository { get; }

    public SufiAbpTenantValidator(ITenantRepository tenantRepository)
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
            throw new InvalidOperationException($"Duplicate tenant name: {tenant.NormalizedName}");
        }
    }
}
