using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiPlatform.Account;

[DependsOn(
    typeof(SufiAccountApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiAccountHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAccountApplicationContractsModule).Assembly,
            AccountRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAccountHttpApiClientModule>();
        });
    }
}
