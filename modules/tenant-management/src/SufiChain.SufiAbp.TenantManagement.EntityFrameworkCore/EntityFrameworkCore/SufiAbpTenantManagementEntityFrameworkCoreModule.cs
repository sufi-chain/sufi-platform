using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpTenantManagementDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAbpTenantManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<TenantManagementDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<Tenant, EfCoreTenantRepository>();
        });
    }
}
