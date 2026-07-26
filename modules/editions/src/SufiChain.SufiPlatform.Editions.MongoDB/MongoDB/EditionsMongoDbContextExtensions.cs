using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Editions.MongoDB;

public static class EditionsMongoDbContextExtensions
{
    public static void ConfigureSufiEditions(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        builder.Entity<Edition>(b =>
        {
            b.CollectionName = EditionsDbProperties.DbTablePrefix + "Editions";
        });
    }
}
