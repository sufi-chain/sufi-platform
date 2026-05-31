using MongoDB.Driver;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.MenuManagement.MongoDB;

[ConnectionStringName(MenuManagementDbProperties.ConnectionStringName)]
public class MenuManagementMongoDbContext : AbpMongoDbContext, IMenuManagementMongoDbContext
{
    public IMongoCollection<Menu> Menus => Collection<Menu>();
    public IMongoCollection<MenuItem> MenuItems => Collection<MenuItem>();
    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        modelBuilder.Entity<Menu>(b => b.CollectionName = MenuManagementDbProperties.DbTablePrefix + "Menus");
        modelBuilder.Entity<MenuItem>(b => b.CollectionName = MenuManagementDbProperties.DbTablePrefix + "MenuItems");
    }
}
