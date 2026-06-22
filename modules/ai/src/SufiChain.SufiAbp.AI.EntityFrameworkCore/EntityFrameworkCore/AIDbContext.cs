using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.AI.MCP.Entities;
using SufiChain.SufiAbp.AI;

namespace SufiChain.SufiAbp.AI.EntityFrameworkCore;

[ConnectionStringName(AIDbProperties.ConnectionStringName)]
public class AIDbContext : AbpDbContext<AIDbContext>, IAIDbContext
{
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<MCPServer> MCPServers { get; set; }
    public DbSet<AIModelConfiguration> AIModelConfigurations { get; set; }
    public DbSet<AIUsageLog> AIUsageLogs { get; set; }

    public AIDbContext(DbContextOptions<AIDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSufiAI();
    }
}
