using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerApplicationContractsModule),
    typeof(SufiAbpHttpClientModule))]
public class SufiAbpFileManagerHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpFileManagerApplicationContractsModule).Assembly,
            FileManagerRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpFileManagerHttpApiClientModule>();
        });

    }
}
