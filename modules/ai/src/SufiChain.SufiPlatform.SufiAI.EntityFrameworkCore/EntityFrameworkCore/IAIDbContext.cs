using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.RAG;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

[ConnectionStringName(SufiAIDbProperties.ConnectionStringName)]
public interface IAIDbContext : IEfCoreDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<AIModelConfiguration> AIModelConfigurations { get; }
    DbSet<AIUsageLog> AIUsageLogs { get; }
    DbSet<MCPServer> MCPServers { get; }
    DbSet<RagIndexingState> RagIndexingStates { get; }
}
