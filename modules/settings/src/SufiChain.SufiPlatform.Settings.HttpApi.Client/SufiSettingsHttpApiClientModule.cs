using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Settings;

[DependsOn(
    typeof(SufiSettingsApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiSettingsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiSettingsApplicationContractsModule).Assembly,
            SettingsRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiSettingsHttpApiClientModule>();
        });
    }
}
