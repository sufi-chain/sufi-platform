using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiPlatform.Permissions;

[DependsOn(
    typeof(SufiPermissionsApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiPermissionsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiPermissionsApplicationContractsModule).Assembly,
            PermissionsRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiPermissionsHttpApiClientModule>();
        });
    }
}
