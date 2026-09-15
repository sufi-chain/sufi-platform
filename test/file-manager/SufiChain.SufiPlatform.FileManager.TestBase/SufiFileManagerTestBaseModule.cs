using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

using Volo.Abp.Autofac;
using Volo.Abp.Guids;
using Volo.Abp.Testing;
namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SufiAuthorizationModule),
    typeof(AbpGuidsModule)
)]
public class SufiFileManagerTestBaseModule : AbpModule
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
                var fileStructureRepository = scope.ServiceProvider.GetService<IFileStructureRepository>();
                if (fileStructureRepository == null)
                {
                    return;
                }

                await scope.ServiceProvider
                    .GetRequiredService<IDataSeeder>()
                    .SeedAsync();
            }
        });
    }
}