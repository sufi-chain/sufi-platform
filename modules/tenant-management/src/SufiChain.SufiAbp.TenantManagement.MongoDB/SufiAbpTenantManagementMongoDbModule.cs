using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.TenantManagement.MongoDB;

[DependsOn(
    typeof(SufiAbpTenantManagementDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiAbpTenantManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<TenantManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<Tenant, MongoTenantRepository>();
        });
    }
}
