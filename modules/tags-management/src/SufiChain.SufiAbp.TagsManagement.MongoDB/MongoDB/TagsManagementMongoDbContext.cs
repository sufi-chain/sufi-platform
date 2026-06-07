using MongoDB.Driver;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.TagsManagement.MongoDB;

[ConnectionStringName(TagsManagementDbProperties.ConnectionStringName)]
public class TagsManagementMongoDbContext : AbpMongoDbContext, ITagsManagementMongoDbContext
{
    public IMongoCollection<Tags.Tag> Tags => Collection<Tags.Tag>();
    public IMongoCollection<TagLink> TagLinks => Collection<TagLink>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiAbpTagsManagement();
    }
}
