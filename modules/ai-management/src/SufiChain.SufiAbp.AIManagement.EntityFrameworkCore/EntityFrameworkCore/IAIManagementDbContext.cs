using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.AIManagement.MCP.Entities;

namespace SufiChain.SufiAbp.AIManagement.EntityFrameworkCore;

[ConnectionStringName(AIManagementDbProperties.ConnectionStringName)]
public interface IAIManagementDbContext : IEfCoreDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<MCPServer> MCPServers { get; }
}
