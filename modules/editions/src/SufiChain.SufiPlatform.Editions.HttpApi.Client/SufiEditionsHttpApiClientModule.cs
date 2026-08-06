using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Editions;

[DependsOn(
    typeof(SufiEditionsApplicationContractsModule),
    typeof(AbpHttpClientModule)
)]
public class SufiEditionsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiEditionsApplicationContractsModule).Assembly,
            EditionsRemoteServiceConsts.RemoteServiceName);

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiEditionsHttpApiClientModule>();
        });
    }
}
