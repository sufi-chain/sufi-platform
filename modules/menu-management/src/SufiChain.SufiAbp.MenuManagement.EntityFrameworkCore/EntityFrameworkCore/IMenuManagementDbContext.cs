using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.MenuManagement.EntityFrameworkCore;

[ConnectionStringName(MenuManagementDbProperties.ConnectionStringName)]
public interface IMenuManagementDbContext : IEfCoreDbContext
{
    DbSet<Menu> Menus { get; }
    DbSet<MenuItem> MenuItems { get; }
}
