using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiPlatform.Menus.EntityFrameworkCore;

public static class MenusDbContextModelCreatingExtensions
{
    public static void ConfigureSufiMenus(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Menu>(b =>
        {
            b.ToTable(SufiMenusDbProperties.DbTablePrefix + "Menus", SufiMenusDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContextType).IsRequired().HasMaxLength(MenusConsts.MaxContextTypeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(MenusConsts.MaxMenuNameLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(MenusConsts.MaxDisplayNameLength);
            b.Property(x => x.Description).HasMaxLength(MenusConsts.MaxDescriptionLength);
            b.HasIndex(x => new { x.TenantId, x.ContextType, x.ContextId, x.Name }).IsUnique();
        });

        builder.Entity<MenuItem>(b =>
        {
            b.ToTable(SufiMenusDbProperties.DbTablePrefix + "MenuItems", SufiMenusDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(MenusConsts.MaxItemNameLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(MenusConsts.MaxDisplayNameLength);
            b.Property(x => x.Slug).IsRequired().HasMaxLength(MenusConsts.MaxSlugLength);
            b.Property(x => x.Description).HasMaxLength(MenusConsts.MaxDescriptionLength);
            b.Property(x => x.Url).HasMaxLength(MenusConsts.MaxUrlLength);
            b.Property(x => x.TargetType).HasMaxLength(MenusConsts.MaxTargetTypeLength);
            b.Property(x => x.Icon).HasMaxLength(MenusConsts.MaxIconLength);
            b.Property(x => x.CssClass).HasMaxLength(MenusConsts.MaxCssClassLength);
            b.Property(x => x.PermissionName).HasMaxLength(MenusConsts.MaxPermissionNameLength);
            b.Property(x => x.ComponentName).HasMaxLength(MenusConsts.MaxComponentNameLength);
            b.Property(x => x.MetadataJson).HasMaxLength(MenusConsts.MaxMetadataJsonLength);
            b.HasIndex(x => new { x.TenantId, x.MenuId, x.Slug }).IsUnique();
            b.HasIndex(x => new { x.MenuId, x.ParentId, x.DisplayOrder });
            b.HasIndex(x => new { x.TargetType, x.TargetId });
            b.HasIndex(x => new { x.MenuId, x.IsActive, x.IsVisible, x.DisplayOrder });
        });
    }
}