using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.FeatureManagement;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.FeatureManagement;

[DependsOn(
    typeof(SufiAbpFeatureManagementApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiAbpFeatureManagementHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpFeatureManagementApplicationContractsModule).Assembly,
            FeatureManagementRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpFeatureManagementHttpApiClientModule>();
        });
    }
}
