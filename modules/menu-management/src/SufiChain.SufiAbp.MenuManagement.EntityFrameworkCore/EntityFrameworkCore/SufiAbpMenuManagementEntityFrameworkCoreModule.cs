using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement.EntityFrameworkCore;

[DependsOn(typeof(SufiAbpMenuManagementDomainModule), typeof(AbpEntityFrameworkCoreModule))]
public class SufiAbpMenuManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<MenuManagementDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Menu, Repositories.EfCoreMenuRepository>();
            options.AddRepository<MenuItem, Repositories.EfCoreMenuItemRepository>();
        });
    }
}
