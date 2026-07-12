using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiTenantsDbProperties.ConnectionStringName)]
public interface ITenantsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Tenant> Tenants { get; }
}
