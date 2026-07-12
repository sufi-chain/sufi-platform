using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Menus.MongoDB;

public static class MenusMongoDbContextExtensions
{
    public static void ConfigureSufiMenus(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Menu>(b =>
        {
            b.CollectionName = SufiMenusDbProperties.DbTablePrefix + "Menus";
        });

        builder.Entity<MenuItem>(b =>
        {
            b.CollectionName = SufiMenusDbProperties.DbTablePrefix + "MenuItems";
        });
    }
}