using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Account;

[DependsOn(
    typeof(SufiAbpAccountApplicationContractsModule),
    typeof(SufiAbpHttpClientModule))]
public class SufiAbpAccountHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpAccountApplicationContractsModule).Assembly,
            AccountRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpAccountHttpApiClientModule>();
        });
    }
}
