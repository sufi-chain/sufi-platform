using MongoDB.Driver;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.MongoDB;

[ConnectionStringName(SufiTagsDbProperties.ConnectionStringName)]
public class TagsMongoDbContext : AbpMongoDbContext, ITagsMongoDbContext
{
    public IMongoCollection<Tags.Tag> Tags => Collection<Tags.Tag>();
    public IMongoCollection<TagLink> TagLinks => Collection<TagLink>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiTags();
    }
}