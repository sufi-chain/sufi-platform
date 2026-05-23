using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.AIManagement.Workspaces;

namespace SufiChain.SufiAbp.AIManagement.MongoDB;

[ConnectionStringName(AIManagementDbProperties.ConnectionStringName)]
public interface IAIManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Workspace> Workspaces { get; }
}
