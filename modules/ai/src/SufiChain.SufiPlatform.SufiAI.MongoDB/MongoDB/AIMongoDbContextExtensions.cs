using Volo.Abp;
using MongoDB.Bson;
using MongoDB.Driver;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.MongoDB;

public static class AIMongoDbContextExtensions
{
    public static void ConfigureSufiAI(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<KnowledgeRelationProposal>(b =>
        {
            b.CollectionName = SufiAIDbProperties.DbTablePrefix + "KnowledgeRelationProposals";
            b.ConfigureIndexes(indexes =>
            {
                indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(nameof(KnowledgeRelationProposal.TenantScopeKey))
                        .Ascending(nameof(KnowledgeRelationProposal.ProposalId)).Ascending(nameof(KnowledgeRelationProposal.ProposalVersion)),
                    new CreateIndexOptions { Name = "IX_KnowledgeProposal_Version", Unique = true }));
                indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(nameof(KnowledgeRelationProposal.TenantScopeKey))
                        .Ascending(nameof(KnowledgeRelationProposal.RequestId)),
                    new CreateIndexOptions { Name = "IX_KnowledgeProposal_Request", Unique = true }));
                indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(nameof(KnowledgeRelationProposal.TenantId))
                        .Ascending(nameof(KnowledgeRelationProposal.State)).Ascending(nameof(KnowledgeRelationProposal.ProposedAtUtc)),
                    new CreateIndexOptions { Name = "IX_KnowledgeProposal_Queue" }));
            });
        });

        builder.Entity<KnowledgeRelationApplicationIntent>(b =>
        {
            b.CollectionName = SufiAIDbProperties.DbTablePrefix + "KnowledgeRelationApplicationIntents";
            b.ConfigureIndexes(indexes =>
            {
                indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(nameof(KnowledgeRelationApplicationIntent.TenantScopeKey))
                        .Ascending(nameof(KnowledgeRelationApplicationIntent.ProposalId))
                        .Ascending(nameof(KnowledgeRelationApplicationIntent.ProposalVersion)),
                    new CreateIndexOptions { Name = "IX_KnowledgeIntent_Proposal", Unique = true }));
                indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(nameof(KnowledgeRelationApplicationIntent.TenantId))
                        .Ascending(nameof(KnowledgeRelationApplicationIntent.State))
                        .Ascending(nameof(KnowledgeRelationApplicationIntent.CreatedAtUtc)),
                    new CreateIndexOptions { Name = "IX_KnowledgeIntent_Queue" }));
            });
        });

        builder.Entity<Workspace>(b =>
        {
            b.CollectionName = SufiAIDbProperties.DbTablePrefix + "Workspaces";
        });
        builder.Entity<WorkspaceAssignment>(b =>
        {
            b.CollectionName = SufiAIDbProperties.DbTablePrefix + "WorkspaceAssignments";
        });
    }
}
