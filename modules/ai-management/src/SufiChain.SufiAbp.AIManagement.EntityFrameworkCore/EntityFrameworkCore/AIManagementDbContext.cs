using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.AIManagement.MCP.Entities;
using SufiChain.SufiAbp.AIManagement.AI;

namespace SufiChain.SufiAbp.AIManagement.EntityFrameworkCore;

[ConnectionStringName(AIManagementDbProperties.ConnectionStringName)]
public class AIManagementDbContext : AbpDbContext<AIManagementDbContext>, IAIManagementDbContext
{
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<MCPServer> MCPServers { get; set; }
    public DbSet<AIModelConfiguration> AIModelConfigurations { get; set; }
    public DbSet<AIUsageLog> AIUsageLogs { get; set; }

    public AIManagementDbContext(DbContextOptions<AIManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureAIManagement();
    }
}
