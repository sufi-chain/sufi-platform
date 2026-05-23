using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorApplicationContractsModule),
    typeof(SufiAbpHttpClientModule))]
public class SufiAbpShortLinkGeneratorHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpShortLinkGeneratorApplicationContractsModule).Assembly,
            ShortLinkGeneratorRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpShortLinkGeneratorHttpApiClientModule>();
        });
    }
}


