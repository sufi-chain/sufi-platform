using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.RAG;

namespace SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

[ConnectionStringName(SufiAIDbProperties.ConnectionStringName)]
public interface IAIDbContext : IEfCoreDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<MCPServer> MCPServers { get; }
    DbSet<RagIndexingState> RagIndexingStates { get; }
}
