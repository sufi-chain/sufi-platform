using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Localization;

[DependsOn(
    typeof(SufiLocalizationApplicationContractsModule),
    typeof(AbpHttpClientModule)
)]
public class SufiLocalizationHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiLocalizationApplicationContractsModule).Assembly,
            LocalizationRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiLocalizationHttpApiClientModule>();
        });
    }
}
