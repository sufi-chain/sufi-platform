using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.AI.MongoDB;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiAIMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<AIMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Workspaces.Workspace, Workspaces.MongoWorkspaceRepository>();
        });
    }
}
