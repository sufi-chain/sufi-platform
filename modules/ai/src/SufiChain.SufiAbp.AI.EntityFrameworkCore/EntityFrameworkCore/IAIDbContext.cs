using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.AI.MCP.Entities;
using SufiChain.SufiAbp.AI.RAG;

namespace SufiChain.SufiAbp.AI.EntityFrameworkCore;

[ConnectionStringName(AIDbProperties.ConnectionStringName)]
public interface IAIDbContext : IEfCoreDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<MCPServer> MCPServers { get; }
    DbSet<RagIndexingState> RagIndexingStates { get; }
}
