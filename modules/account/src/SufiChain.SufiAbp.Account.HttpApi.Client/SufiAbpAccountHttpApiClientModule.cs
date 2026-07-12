using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiAbp.Account;

[DependsOn(
    typeof(SufiAbpAccountApplicationContractsModule),
    typeof(AbpHttpClientModule))]
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
