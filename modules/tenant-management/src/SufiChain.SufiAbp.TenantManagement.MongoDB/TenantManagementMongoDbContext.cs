using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.TenantManagement.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpTenantManagementDbProperties.ConnectionStringName)]
public class TenantManagementMongoDbContext : AbpMongoDbContext, ITenantManagementMongoDbContext
{
    public IMongoCollection<Tenant> Tenants => Collection<Tenant>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiAbpTenantManagement();
    }
}
