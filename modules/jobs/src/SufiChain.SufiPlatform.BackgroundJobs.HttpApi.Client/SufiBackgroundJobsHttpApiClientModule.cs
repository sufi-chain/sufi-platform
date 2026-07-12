using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.BackgroundJobs;

[DependsOn(
    typeof(SufiBackgroundJobsApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiBackgroundJobsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiBackgroundJobsApplicationContractsModule).Assembly,
            BackgroundJobsRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiBackgroundJobsHttpApiClientModule>();
        });
    }
}
