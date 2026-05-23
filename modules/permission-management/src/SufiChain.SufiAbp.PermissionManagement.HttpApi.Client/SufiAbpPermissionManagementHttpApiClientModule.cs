using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.PermissionManagement;

[DependsOn(
    typeof(SufiAbpPermissionManagementApplicationContractsModule),
    typeof(SufiAbpHttpClientModule))]
public class SufiAbpPermissionManagementHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpPermissionManagementApplicationContractsModule).Assembly,
            PermissionManagementRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpPermissionManagementHttpApiClientModule>();
        });
    }
}
