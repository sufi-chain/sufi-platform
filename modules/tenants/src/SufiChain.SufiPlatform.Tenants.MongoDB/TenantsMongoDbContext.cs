using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiTenantsDbProperties.ConnectionStringName)]
public class TenantsMongoDbContext : AbpMongoDbContext, ITenantsMongoDbContext
{
    public IMongoCollection<Tenant> Tenants => Collection<Tenant>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiTenants();
    }
}
