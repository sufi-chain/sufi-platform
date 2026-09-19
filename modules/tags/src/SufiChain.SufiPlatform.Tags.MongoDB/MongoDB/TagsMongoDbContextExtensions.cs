using SufiChain.SufiPlatform.Tags.Tags;
using MongoDB.Bson;
using MongoDB.Driver;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.MongoDB;

public static class TagsMongoDbContextExtensions
{
    public static void ConfigureSufiTags(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<TagRelationDefinition>(b =>
        {
            b.CollectionName = SufiTagsDbProperties.DbTablePrefix + "RelationDefinitions";
            b.ConfigureIndexes(indexes =>
            {
                indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(nameof(TagRelationDefinition.TenantScopeKey))
                        .Ascending(nameof(TagRelationDefinition.StableKey)).Ascending(nameof(TagRelationDefinition.Revision)),
                    new CreateIndexOptions { Name = "IX_RelationDefinition_Key_Revision", Unique = true }));
                indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(nameof(TagRelationDefinition.TenantScopeKey))
                        .Ascending(nameof(TagRelationDefinition.PredicateTagId)).Ascending(nameof(TagRelationDefinition.Revision)),
                    new CreateIndexOptions { Name = "IX_RelationDefinition_Tag_Revision", Unique = true }));
            });
        });

        builder.Entity<EntityRelation>(b =>
        {
            b.CollectionName = SufiTagsDbProperties.DbTablePrefix + "Relations";
            b.ConfigureIndexes(indexes => indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending(nameof(EntityRelation.TenantScopeKey))
                    .Ascending(nameof(EntityRelation.ScopeType)).Ascending(nameof(EntityRelation.ScopeId))
                    .Ascending(nameof(EntityRelation.PredicateTagId)).Ascending(nameof(EntityRelation.SourceType))
                    .Ascending(nameof(EntityRelation.SourceId)).Ascending(nameof(EntityRelation.TargetType))
                    .Ascending(nameof(EntityRelation.TargetId)),
                new CreateIndexOptions { Name = "IX_Relation_Identity", Unique = true })));
        });

        builder.Entity<TagRelationMutationReceipt>(b =>
        {
            b.CollectionName = SufiTagsDbProperties.DbTablePrefix + "RelationMutationReceipts";
            b.ConfigureIndexes(indexes => indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending(nameof(TagRelationMutationReceipt.TenantScopeKey))
                    .Ascending(nameof(TagRelationMutationReceipt.RelationId))
                    .Ascending(nameof(TagRelationMutationReceipt.AppliedVersion)),
                new CreateIndexOptions { Name = "IX_RelationReceipt_Version" })));
        });

        builder.Entity<Tags.Tag>(b =>
        {
            b.CollectionName = SufiTagsDbProperties.DbTablePrefix + "Tags";
        });

        builder.Entity<TagLink>(b =>
        {
            b.CollectionName = SufiTagsDbProperties.DbTablePrefix + "TagLinks";
        });
    }
}
