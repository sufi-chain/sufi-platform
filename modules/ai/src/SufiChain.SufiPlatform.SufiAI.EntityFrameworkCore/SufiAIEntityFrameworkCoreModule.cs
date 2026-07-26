using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAIEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AIDbContext>(options =>
        {
            options.AddDefaultRepositories<IAIDbContext>(includeAllEntities: true);
            options.AddRepository<Workspaces.Workspace, Workspaces.EfCoreWorkspaceRepository>();
            options.AddRepository<AIModelConfiguration, EfCoreAIModelConfigurationRepository>();
            options.AddRepository<AIUsageLog, EfCoreAIUsageLogRepository>();
            options.AddRepository<MCP.Entities.MCPServer, MCPServerRepository>();
        });
    }
}
