using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.MongoDB;

[ConnectionStringName(SufiAIDbProperties.ConnectionStringName)]
public class AIMongoDbContext : AbpMongoDbContext, IAIMongoDbContext
{
    public IMongoCollection<Workspace> Workspaces => Collection<Workspace>();
    public IMongoCollection<Knowledge.KnowledgeRelationProposal> KnowledgeRelationProposals => Collection<Knowledge.KnowledgeRelationProposal>();
    public IMongoCollection<Knowledge.KnowledgeRelationApplicationIntent> KnowledgeRelationApplicationIntents => Collection<Knowledge.KnowledgeRelationApplicationIntent>();
    public IMongoCollection<WorkspaceAssignment> WorkspaceAssignments => Collection<WorkspaceAssignment>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiAI();
    }
}
