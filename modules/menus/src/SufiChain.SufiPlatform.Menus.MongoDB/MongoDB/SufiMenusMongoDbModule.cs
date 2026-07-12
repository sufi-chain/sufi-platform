using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Menus.MongoDB;

[DependsOn(typeof(SufiMenusDomainModule), typeof(AbpMongoDbModule))]
public class SufiMenusMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<MenusMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Menu, Repositories.MongoMenuRepository>();
            options.AddRepository<MenuItem, Repositories.MongoMenuItemRepository>();
        });
    }
}