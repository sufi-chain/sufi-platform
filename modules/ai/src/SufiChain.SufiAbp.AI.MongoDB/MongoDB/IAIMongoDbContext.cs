using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.AI.Workspaces;

namespace SufiChain.SufiAbp.AI.MongoDB;

[ConnectionStringName(AIDbProperties.ConnectionStringName)]
public interface IAIMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Workspace> Workspaces { get; }
}
