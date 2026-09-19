using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.MongoDB;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiAIMongoDbModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        BsonClassMap.TryRegisterClassMap<KnowledgeRelationProposal>(map =>
        {
            map.AutoMap();
            var utc = new DateTimeSerializer(DateTimeKind.Utc);
            map.MapMember(x => x.ProposedAtUtc).SetSerializer(utc);
            map.MapMember(x => x.ReviewedAtUtc).SetSerializer(new NullableSerializer<DateTime>(utc));
            map.MapMember(x => x.ExpiresAtUtc).SetSerializer(new NullableSerializer<DateTime>(utc));
            map.MapMember(x => x.RevokedAtUtc).SetSerializer(new NullableSerializer<DateTime>(utc));
        });
        BsonClassMap.TryRegisterClassMap<KnowledgeRelationApplicationIntent>(map =>
        {
            map.AutoMap();
            var utc = new DateTimeSerializer(DateTimeKind.Utc);
            map.MapMember(x => x.CreatedAtUtc).SetSerializer(utc);
            map.MapMember(x => x.AppliedAtUtc).SetSerializer(new NullableSerializer<DateTime>(utc));
            map.MapMember(x => x.NeedsReviewAtUtc).SetSerializer(new NullableSerializer<DateTime>(utc));
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<AIMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Workspaces.Workspace, Workspaces.MongoWorkspaceRepository>();
            options.AddRepository<Workspaces.WorkspaceAssignment, Workspaces.MongoWorkspaceAssignmentRepository>();
            options.AddRepository<Knowledge.KnowledgeRelationProposal, Knowledge.MongoKnowledgeRelationProposalRepository>();
            options.AddRepository<Knowledge.KnowledgeRelationApplicationIntent, Knowledge.MongoKnowledgeRelationApplicationIntentRepository>();
        });
    }
}
