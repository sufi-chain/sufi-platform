using MongoDB.Driver;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.MongoDB;

[ConnectionStringName(SufiTagsDbProperties.ConnectionStringName)]
public class TagsMongoDbContext : AbpMongoDbContext, ITagsMongoDbContext
{
    public IMongoCollection<Tags.Tag> Tags => Collection<Tags.Tag>();
    public IMongoCollection<TagLink> TagLinks => Collection<TagLink>();
    public IMongoCollection<TagRelationDefinition> RelationDefinitions => Collection<TagRelationDefinition>();
    public IMongoCollection<EntityRelation> Relations => Collection<EntityRelation>();
    public IMongoCollection<TagRelationMutationReceipt> RelationMutationReceipts => Collection<TagRelationMutationReceipt>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiTags();
    }
}
