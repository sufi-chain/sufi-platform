using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(ShortLinkGeneratorApplicationContractsModule),
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpTestBaseModule),
    typeof(SufiAbpAuthorizationModule),
    typeof(SufiAbpGuidsModule)
)]
public class SufiAbpShortLinkGeneratorTestBaseModule : AbpModule
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
        using (var scope = context.ServiceProvider.CreateScope())
        {
            // Seed test data here
        }
    }
}
