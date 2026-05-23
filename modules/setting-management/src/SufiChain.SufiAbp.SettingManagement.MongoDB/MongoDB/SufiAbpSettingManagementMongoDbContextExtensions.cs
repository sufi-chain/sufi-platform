using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.MongoDB;

public static class SufiAbpSettingManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpSettingManagement(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Setting>(b =>
        {
            b.CollectionName = SufiAbpSettingManagementDbProperties.DbTablePrefix + "Settings";
        });

        builder.Entity<SettingDefinitionRecord>(b =>
        {
            b.CollectionName = SufiAbpSettingManagementDbProperties.DbTablePrefix + "SettingDefinitionRecords";
        });
    }
}
