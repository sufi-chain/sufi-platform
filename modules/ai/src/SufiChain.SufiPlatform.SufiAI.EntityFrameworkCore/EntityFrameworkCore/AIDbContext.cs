using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.RAG;

namespace SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

[ConnectionStringName(SufiAIDbProperties.ConnectionStringName)]
public class AIDbContext : AbpDbContext<AIDbContext>, IAIDbContext
{
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<MCPServer> MCPServers { get; set; }
    public DbSet<AIModelConfiguration> AIModelConfigurations { get; set; }
    public DbSet<AIUsageLog> AIUsageLogs { get; set; }
    public DbSet<RagIndexingState> RagIndexingStates { get; set; }

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
