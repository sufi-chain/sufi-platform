using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.TagsManagement.MongoDB;

public static class TagsManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpTagsManagement(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Tag>(b =>
        {
            b.CollectionName = TagsManagementDbProperties.DbTablePrefix + "Tags";
        });

        builder.Entity<TagLink>(b =>
        {
            b.CollectionName = TagsManagementDbProperties.DbTablePrefix + "TagLinks";
        });
    }
}
