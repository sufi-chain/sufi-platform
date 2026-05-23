using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.AIManagement.EntityFrameworkCore;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAbpAIManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AIManagementDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Workspaces.Workspace, Workspaces.EfCoreWorkspaceRepository>();
        });
    }
}
