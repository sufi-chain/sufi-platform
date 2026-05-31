using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiAbp.MenuManagement.EntityFrameworkCore;

[ConnectionStringName(MenuManagementDbProperties.ConnectionStringName)]
public class MenuManagementDbContext : AbpDbContext<MenuManagementDbContext>, IMenuManagementDbContext
{
    public DbSet<Menu> Menus { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public MenuManagementDbContext(DbContextOptions<MenuManagementDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Menu>(b =>
        {
            b.ToTable(MenuManagementDbProperties.DbTablePrefix + "Menus", MenuManagementDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContextType).IsRequired().HasMaxLength(MenuManagementConsts.MaxContextTypeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(MenuManagementConsts.MaxMenuNameLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(MenuManagementConsts.MaxDisplayNameLength);
            b.Property(x => x.Description).HasMaxLength(MenuManagementConsts.MaxDescriptionLength);
            b.HasIndex(x => new { x.TenantId, x.ContextType, x.ContextId, x.Name }).IsUnique();
        });
        builder.Entity<MenuItem>(b =>
        {
            b.ToTable(MenuManagementDbProperties.DbTablePrefix + "MenuItems", MenuManagementDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(MenuManagementConsts.MaxItemNameLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(MenuManagementConsts.MaxDisplayNameLength);
            b.Property(x => x.Slug).IsRequired().HasMaxLength(MenuManagementConsts.MaxSlugLength);
            b.Property(x => x.Description).HasMaxLength(MenuManagementConsts.MaxDescriptionLength);
            b.Property(x => x.Url).HasMaxLength(MenuManagementConsts.MaxUrlLength);
            b.Property(x => x.TargetType).HasMaxLength(MenuManagementConsts.MaxTargetTypeLength);
            b.Property(x => x.Icon).HasMaxLength(MenuManagementConsts.MaxIconLength);
            b.Property(x => x.CssClass).HasMaxLength(MenuManagementConsts.MaxCssClassLength);
            b.Property(x => x.PermissionName).HasMaxLength(MenuManagementConsts.MaxPermissionNameLength);
            b.Property(x => x.ComponentName).HasMaxLength(MenuManagementConsts.MaxComponentNameLength);
            b.Property(x => x.MetadataJson).HasMaxLength(MenuManagementConsts.MaxMetadataJsonLength);
            b.HasIndex(x => new { x.TenantId, x.MenuId, x.Slug }).IsUnique();
            b.HasIndex(x => new { x.MenuId, x.ParentId, x.DisplayOrder });
            b.HasIndex(x => new { x.TargetType, x.TargetId });
            b.HasIndex(x => new { x.MenuId, x.IsActive, x.IsVisible, x.DisplayOrder });
        });
    }
}
