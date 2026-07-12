using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.ShortLinks.MongoDB.MongoDB;

public static class SufiShortLinksMongoDbContextExtensions
{
    public static void ConfigureSufiShortLinks(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<ShortUrl>(b =>
        {
            b.CollectionName = SufiShortLinksDbProperties.DbTablePrefix + "ShortUrls";
        });

        builder.Entity<ShortUrlClick>(b =>
        {
            b.CollectionName = SufiShortLinksDbProperties.DbTablePrefix + "ShortUrlClicks";
        });
    }
}