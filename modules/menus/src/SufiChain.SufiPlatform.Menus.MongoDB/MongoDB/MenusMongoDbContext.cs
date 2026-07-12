using MongoDB.Driver;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Menus.MongoDB;

[ConnectionStringName(SufiMenusDbProperties.ConnectionStringName)]
public class MenusMongoDbContext : AbpMongoDbContext, IMenusMongoDbContext
{
    public IMongoCollection<Menu> Menus => Collection<Menu>();
    public IMongoCollection<MenuItem> MenuItems => Collection<MenuItem>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiMenus();
    }
}