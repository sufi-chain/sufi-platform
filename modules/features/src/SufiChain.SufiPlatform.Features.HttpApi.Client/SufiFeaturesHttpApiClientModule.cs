using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Features;

[DependsOn(
    typeof(SufiFeaturesApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiFeaturesHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiFeaturesApplicationContractsModule).Assembly,
            FeaturesRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiFeaturesHttpApiClientModule>();
        });
    }
}
