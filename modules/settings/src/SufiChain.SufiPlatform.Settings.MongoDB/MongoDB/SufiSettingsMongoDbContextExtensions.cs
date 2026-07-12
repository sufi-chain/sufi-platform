using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.MongoDB;

public static class SufiSettingsMongoDbContextExtensions
{
    public static void ConfigureSufiSettings(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Setting>(b =>
        {
            b.CollectionName = SufiSettingsDbProperties.DbTablePrefix + "Settings";
        });

        builder.Entity<SettingDefinitionRecord>(b =>
        {
            b.CollectionName = SufiSettingsDbProperties.DbTablePrefix + "SettingDefinitionRecords";
        });
    }
}
