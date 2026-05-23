using Volo.Abp;
using SufiChain.SufiAbp.FeatureManagement;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.FeatureManagement.MongoDB;

public static class SufiAbpFeatureManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpFeatureManagement(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<FeatureGroupDefinitionRecord>(b =>
        {
            b.CollectionName = SufiAbpFeatureManagementDbProperties.DbTablePrefix + "FeatureGroups";
        });

        builder.Entity<FeatureDefinitionRecord>(b =>
        {
            b.CollectionName = SufiAbpFeatureManagementDbProperties.DbTablePrefix + "Features";
        });

        builder.Entity<FeatureValue>(b =>
        {
            b.CollectionName = SufiAbpFeatureManagementDbProperties.DbTablePrefix + "FeatureValues";
        });
    }
}
