using Volo.Abp.Application;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Mapperly;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
