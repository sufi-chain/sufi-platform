using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.MongoDB;

public static class SufiAbpShortLinkGeneratorMongoDbContextExtensions
{
    public static void ConfigureSufiAbpShortLinkGenerator(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<ShortUrl>(b =>
        {
            b.CollectionName = SufiAbpShortLinkGeneratorDbProperties.DbTablePrefix + "ShortUrls";
        });

        builder.Entity<ShortUrlClick>(b =>
        {
            b.CollectionName = SufiAbpShortLinkGeneratorDbProperties.DbTablePrefix + "ShortUrlClicks";
        });
    }
}
