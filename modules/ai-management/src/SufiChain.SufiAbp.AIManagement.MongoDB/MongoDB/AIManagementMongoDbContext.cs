using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.AIManagement.Workspaces;

namespace SufiChain.SufiAbp.AIManagement.MongoDB;

[ConnectionStringName(AIManagementDbProperties.ConnectionStringName)]
public class AIManagementMongoDbContext : AbpMongoDbContext, IAIManagementMongoDbContext
{
    public IMongoCollection<Workspace> Workspaces => Collection<Workspace>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureAIManagement();
    }
}
