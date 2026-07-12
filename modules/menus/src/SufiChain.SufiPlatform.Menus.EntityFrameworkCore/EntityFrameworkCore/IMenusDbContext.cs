using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Menus.EntityFrameworkCore;

[ConnectionStringName(SufiMenusDbProperties.ConnectionStringName)]
public interface IMenusDbContext : IEfCoreDbContext
{
    DbSet<Menu> Menus { get; }
    DbSet<MenuItem> MenuItems { get; }
}