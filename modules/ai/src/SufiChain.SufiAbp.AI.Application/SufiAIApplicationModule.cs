using Volo.Abp.Application;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Mapperly;
using Microsoft.Extensions.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(SufiAIApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule)
)]
public class SufiAIApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAIApplicationModule>();
    }
}
