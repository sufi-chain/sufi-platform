using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tenants.MongoDB;

[DependsOn(
    typeof(SufiTenantsDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiTenantsMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<TenantsMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<Tenant, MongoTenantRepository>();
        });
    }
}
