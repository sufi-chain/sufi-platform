using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.MenuManagement.MongoDB;

[DependsOn(typeof(SufiAbpMenuManagementDomainModule), typeof(AbpMongoDbModule))]
public class SufiAbpMenuManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<MenuManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Menu, Repositories.MongoMenuRepository>();
            options.AddRepository<MenuItem, Repositories.MongoMenuItemRepository>();
        });
    }
}
