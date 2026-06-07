using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.MenuManagement.MongoDB;

public static class MenuManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpMenuManagement(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Menu>(b =>
        {
            b.CollectionName = MenuManagementDbProperties.DbTablePrefix + "Menus";
        });

        builder.Entity<MenuItem>(b =>
        {
            b.CollectionName = MenuManagementDbProperties.DbTablePrefix + "MenuItems";
        });
    }
}
