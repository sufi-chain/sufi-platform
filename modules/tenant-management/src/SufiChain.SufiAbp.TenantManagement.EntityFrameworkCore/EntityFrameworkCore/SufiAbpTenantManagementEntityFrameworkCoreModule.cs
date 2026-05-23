using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpTenantManagementDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
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
