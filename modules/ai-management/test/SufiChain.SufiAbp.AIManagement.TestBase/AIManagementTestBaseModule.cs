using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.TestBase;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementDomainModule),
    typeof(SufiAbpTestBaseModule),
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpAuthorizationModule)
)]
public class AIManagementTestBaseModule : AbpModule
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
