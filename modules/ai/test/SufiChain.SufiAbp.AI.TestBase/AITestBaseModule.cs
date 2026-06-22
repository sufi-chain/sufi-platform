using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.TestBase;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(SufiAbpTestBaseModule),
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpAuthorizationModule)
)]
public class AITestBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        SeedTestData(context);
    }

    private static void SeedTestData(ApplicationInitializationContext context)
    {
        // Seed test data if needed
    }
}
