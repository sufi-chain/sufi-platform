using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.TenantManagement.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpTenantManagementDbProperties.ConnectionStringName)]
public interface ITenantManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Tenant> Tenants { get; }
}
