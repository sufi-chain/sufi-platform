using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.Tenants;

namespace SufiChain.SufiPlatform.Tenants.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiTenantsDbProperties.ConnectionStringName)]
public interface ISufiTenantsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Tenant> Tenants { get; }
}
