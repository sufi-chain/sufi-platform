using MongoDB.Driver;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Menus.MongoDB;

[ConnectionStringName(SufiMenusDbProperties.ConnectionStringName)]
public interface IMenusMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Menu> Menus { get; }
    IMongoCollection<MenuItem> MenuItems { get; }
}