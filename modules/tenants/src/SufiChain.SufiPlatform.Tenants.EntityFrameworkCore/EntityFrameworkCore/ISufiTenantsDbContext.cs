using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiTenantsDbProperties.ConnectionStringName)]
public interface ITenantsDbContext : IEfCoreDbContext
{
    DbSet<Tenant> Tenants { get; }

    DbSet<TenantConnectionString> TenantConnectionStrings { get; }

    DbSet<TenantDomain> TenantDomains { get; }
}
