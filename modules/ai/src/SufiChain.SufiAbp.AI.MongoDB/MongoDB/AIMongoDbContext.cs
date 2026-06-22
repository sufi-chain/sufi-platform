using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.AI.Workspaces;

namespace SufiChain.SufiAbp.AI.MongoDB;

[ConnectionStringName(AIDbProperties.ConnectionStringName)]
public class AIMongoDbContext : AbpMongoDbContext, IAIMongoDbContext
{
    public IMongoCollection<Workspace> Workspaces => Collection<Workspace>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiAI();
    }
}
