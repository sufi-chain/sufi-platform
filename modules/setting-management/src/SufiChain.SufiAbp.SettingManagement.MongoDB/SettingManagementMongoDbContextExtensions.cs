using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.SettingManagement.MongoDB;

public static class SettingManagementMongoDbContextExtensions
{
    public static void ConfigureSettingManagement(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Setting>(b =>
        {
            b.CollectionName = SufiAbpSettingManagementDbProperties.DbTablePrefix + "Settings";
        });

        builder.Entity<SettingDefinitionRecord>(b =>
        {
            b.CollectionName = SufiAbpSettingManagementDbProperties.DbTablePrefix + "SettingDefinitions";
        });
    }
}
