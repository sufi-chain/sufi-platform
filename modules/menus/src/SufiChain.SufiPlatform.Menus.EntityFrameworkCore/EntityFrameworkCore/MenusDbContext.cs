using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Menus.EntityFrameworkCore;

[ConnectionStringName(SufiMenusDbProperties.ConnectionStringName)]
public class MenusDbContext : AbpDbContext<MenusDbContext>, IMenusDbContext
{
    public DbSet<Menu> Menus { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;

    public MenusDbContext(DbContextOptions<MenusDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSufiMenus();
    }
}