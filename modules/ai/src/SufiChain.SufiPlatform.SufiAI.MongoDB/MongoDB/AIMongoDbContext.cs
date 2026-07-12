using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.MongoDB;

[ConnectionStringName(SufiAIDbProperties.ConnectionStringName)]
public class AIMongoDbContext : AbpMongoDbContext, IAIMongoDbContext
{
    public IMongoCollection<Workspace> Workspaces => Collection<Workspace>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSufiAI();
    }
}
