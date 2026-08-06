using SufiChain.SufiPlatform.Localization.Entities;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Localization.MongoDB;

public static class LocalizationMongoDbContextExtensions
{
    public static void ConfigureSufiLocalization(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<LocalizationText>(b =>
        {
            b.CollectionName = SufiLocalizationDbProperties.DbTablePrefix + "LocalizationTexts";
        });

        builder.Entity<LocalizationResource>(b =>
        {
            b.CollectionName = SufiLocalizationDbProperties.DbTablePrefix + "LocalizationResources";
        });
    }
}
