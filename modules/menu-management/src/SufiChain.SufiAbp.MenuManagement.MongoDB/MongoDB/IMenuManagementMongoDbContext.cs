using MongoDB.Driver;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.MenuManagement.MongoDB;

[ConnectionStringName(MenuManagementDbProperties.ConnectionStringName)]
public interface IMenuManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Menu> Menus { get; }
    IMongoCollection<MenuItem> MenuItems { get; }
}
