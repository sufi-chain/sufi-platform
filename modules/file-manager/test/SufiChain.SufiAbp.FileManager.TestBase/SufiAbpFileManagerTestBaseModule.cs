using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.Guids;
using SufiChain.SufiAbp.TestBase;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpTestBaseModule),
    typeof(SufiAbpAuthorizationModule),
    typeof(SufiAbpGuidsModule)
)]
public class SufiAbpFileManagerTestBaseModule : AbpModule
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
        AsyncHelper.RunSync(async () =>
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<IDataSeeder>()
                    .SeedAsync();
            }
        });
    }
}
