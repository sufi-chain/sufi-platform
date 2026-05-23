using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.MongoDB;
using SufiChain.SufiAbp.AIManagement.MongoDB;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementDomainModule),
    typeof(SufiAbpMongoDbModule)
)]
public class SufiAbpAIManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<AIManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Workspaces.Workspace, Workspaces.MongoWorkspaceRepository>();
        });
    }
}
