using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAbpAIManagementHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // Configure auto-generated controllers for WorkspaceAppService and RAGAppService
        PreConfigure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers
                .Create(typeof(SufiAbpAIManagementApplicationContractsModule).Assembly, opts =>
                {
                    opts.RootPath = "ai-management";
                    opts.RemoteServiceName = "AIManagement";
                });
        });
    }
}
