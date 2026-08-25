using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Menus.EntityFrameworkCore;

[DependsOn(typeof(SufiMenusDomainModule), typeof(AbpEntityFrameworkCoreModule))]
public class SufiMenusEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<MenusDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Menu, Repositories.EfCoreMenuRepository>();
            options.AddRepository<MenuItem, Repositories.EfCoreMenuItemRepository>();
        });

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(SufiMenusDbProperties.ConnectionStringName, db =>
            {
                db.IsUsedByTenants = true;
            });
        });
    }
}
