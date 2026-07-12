using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tenants.EntityFrameworkCore;

[DependsOn(
    typeof(SufiTenantsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiTenantsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<TenantsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<Tenant, EfCoreTenantRepository>();
        });
    }
}
