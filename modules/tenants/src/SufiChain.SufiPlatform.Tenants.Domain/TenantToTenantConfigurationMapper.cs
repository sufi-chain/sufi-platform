using Riok.Mapperly.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.Mapperly;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TenantToTenantConfigurationMapper : MapperBase<Tenant, TenantConfiguration>
{
    [MapperIgnoreTarget(nameof(TenantConfiguration.IsActive))]
    [MapperIgnoreTarget(nameof(TenantConfiguration.ConnectionStrings))]
    public override partial TenantConfiguration Map(Tenant source);

    [MapperIgnoreTarget(nameof(TenantConfiguration.IsActive))]
    [MapperIgnoreTarget(nameof(TenantConfiguration.ConnectionStrings))]
    public override partial void Map(Tenant source, TenantConfiguration destination);

    public override void AfterMap(Tenant source, TenantConfiguration destination)
    {
        // Sufi tenants do not carry an activation flag; treat them as always active.
        destination.IsActive = true;

        if (source.ConnectionStrings != null)
        {
            destination.ConnectionStrings = new ConnectionStrings();
            foreach (var connectionString in source.ConnectionStrings)
            {
                destination.ConnectionStrings[connectionString.Name] = connectionString.Value;
            }
        }
    }
}
