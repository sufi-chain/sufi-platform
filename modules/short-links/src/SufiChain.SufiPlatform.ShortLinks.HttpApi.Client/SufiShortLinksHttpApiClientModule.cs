using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.ShortLinks;

[DependsOn(
    typeof(SufiShortLinksApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiShortLinksHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiShortLinksApplicationContractsModule).Assembly,
            ShortLinksRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiShortLinksHttpApiClientModule>();
        });
    }
}