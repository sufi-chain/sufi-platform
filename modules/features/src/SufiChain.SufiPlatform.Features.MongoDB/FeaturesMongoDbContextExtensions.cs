using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Features.MongoDB;

public static class FeaturesMongoDbContextExtensions
{
    public static void ConfigureFeatures(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<FeatureGroupDefinitionRecord>(b =>
        {
            b.CollectionName = SufiFeaturesDbProperties.DbTablePrefix + "FeatureGroups";
        });

        builder.Entity<FeatureDefinitionRecord>(b =>
        {
            b.CollectionName = SufiFeaturesDbProperties.DbTablePrefix + "Features";
        });

        builder.Entity<FeatureValue>(b =>
        {
            b.CollectionName = SufiFeaturesDbProperties.DbTablePrefix + "FeatureValues";
        });
    }
}
