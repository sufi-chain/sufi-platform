using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.LocalizationManagement.MongoDB;

public static class LocalizationManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpLocalizationManagement(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<LocalizationText>(b =>
        {
            b.CollectionName = SufiAbpLocalizationManagementDbProperties.DbTablePrefix + "LocalizationTexts";
        });

        builder.Entity<LocalizationResource>(b =>
        {
            b.CollectionName = SufiAbpLocalizationManagementDbProperties.DbTablePrefix + "LocalizationResources";
        });
    }
}
