using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

using Volo.Abp.Autofac;
using Volo.Abp.Testing;
namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(AbpTestBaseModule),
    typeof(AbpAutofacModule),
    typeof(SufiAuthorizationModule)
)]
public class SufiAITestBaseModule : AbpModule
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
