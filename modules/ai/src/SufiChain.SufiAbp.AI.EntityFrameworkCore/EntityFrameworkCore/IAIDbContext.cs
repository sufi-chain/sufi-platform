using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.AI.MCP.Entities;

namespace SufiChain.SufiAbp.AI.EntityFrameworkCore;

[ConnectionStringName(AIDbProperties.ConnectionStringName)]
public interface IAIDbContext : IEfCoreDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<MCPServer> MCPServers { get; }
}
