using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MenuManagement.Menus;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.MenuManagement.MongoDB;

[DependsOn(typeof(SufiAbpMenuManagementDomainModule), typeof(SufiAbpMongoDbModule))]
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
