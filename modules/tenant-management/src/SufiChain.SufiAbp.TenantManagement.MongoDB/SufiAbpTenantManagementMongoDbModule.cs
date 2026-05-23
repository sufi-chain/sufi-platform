using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.TenantManagement.MongoDB;

[DependsOn(
    typeof(SufiAbpTenantManagementDomainModule),
    typeof(SufiAbpMongoDbModule)
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
