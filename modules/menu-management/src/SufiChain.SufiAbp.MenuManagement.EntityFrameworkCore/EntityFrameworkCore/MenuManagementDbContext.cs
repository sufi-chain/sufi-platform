using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.MenuManagement.EntityFrameworkCore;

[ConnectionStringName(MenuManagementDbProperties.ConnectionStringName)]
public class MenuManagementDbContext : AbpDbContext<MenuManagementDbContext>, IMenuManagementDbContext
{
    public DbSet<Menu> Menus { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;

    public MenuManagementDbContext(DbContextOptions<MenuManagementDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSufiAbpMenuManagement();
    }
}
