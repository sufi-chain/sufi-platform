using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AIManagement.Chat;
using Volo.Abp.Application;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Mapperly;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementDomainModule),
    typeof(SufiAbpAIManagementApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule)
)]
public class SufiAbpAIManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpAIManagementApplicationModule>();
        
        // Register application services
        context.Services.AddTransient<IAIChatAppService, AIChatAppService>();
    }
}
