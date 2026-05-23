using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpTenantManagementDbProperties.ConnectionStringName)]
public interface ITenantManagementDbContext : IEfCoreDbContext
{
    DbSet<Tenant> Tenants { get; }

    DbSet<TenantConnectionString> TenantConnectionStrings { get; }
}
