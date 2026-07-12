using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Settings.MongoDB;

public static class SettingsMongoDbContextExtensions
{
    public static void ConfigureSettings(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Setting>(b =>
        {
            b.CollectionName = SufiSettingsDbProperties.DbTablePrefix + "Settings";
        });

        builder.Entity<SettingDefinitionRecord>(b =>
        {
            b.CollectionName = SufiSettingsDbProperties.DbTablePrefix + "SettingDefinitions";
        });
    }
}
