using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.MongoDB;

public static class TagsMongoDbContextExtensions
{
    public static void ConfigureSufiTags(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Tag>(b =>
        {
            b.CollectionName = SufiTagsDbProperties.DbTablePrefix + "Tags";
        });

        builder.Entity<TagLink>(b =>
        {
            b.CollectionName = SufiTagsDbProperties.DbTablePrefix + "TagLinks";
        });
    }
}