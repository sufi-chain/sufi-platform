using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiFileManagerHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiFileManagerApplicationContractsModule).Assembly,
            FileManagerRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiFileManagerHttpApiClientModule>();
        });

    }
}