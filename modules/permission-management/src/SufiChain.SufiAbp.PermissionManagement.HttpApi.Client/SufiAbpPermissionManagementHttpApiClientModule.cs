using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiAbp.PermissionManagement;

[DependsOn(
    typeof(SufiAbpPermissionManagementApplicationContractsModule),
    typeof(AbpHttpClientModule))]
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
