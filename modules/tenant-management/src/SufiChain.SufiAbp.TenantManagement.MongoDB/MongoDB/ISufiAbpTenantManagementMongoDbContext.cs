using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiAbp.TenantManagement;

namespace SufiChain.SufiAbp.TenantManagement.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpTenantManagementDbProperties.ConnectionStringName)]
public interface ISufiAbpTenantManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Tenant> Tenants { get; }
}
