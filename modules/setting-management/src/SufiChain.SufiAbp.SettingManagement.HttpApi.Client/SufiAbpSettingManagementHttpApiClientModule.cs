using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.SettingManagement;

[DependsOn(
    typeof(SufiAbpSettingManagementApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiAbpSettingManagementHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpSettingManagementApplicationContractsModule).Assembly,
            SettingManagementRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpSettingManagementHttpApiClientModule>();
        });
    }
}
