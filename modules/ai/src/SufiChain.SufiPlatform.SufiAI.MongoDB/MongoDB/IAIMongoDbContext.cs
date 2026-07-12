using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.MongoDB;

[ConnectionStringName(SufiAIDbProperties.ConnectionStringName)]
public interface IAIMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Workspace> Workspaces { get; }
}
