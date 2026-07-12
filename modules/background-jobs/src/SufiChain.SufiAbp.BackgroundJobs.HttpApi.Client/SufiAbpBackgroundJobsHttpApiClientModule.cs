using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.BackgroundJobs;

[DependsOn(
    typeof(SufiAbpBackgroundJobsApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiAbpBackgroundJobsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpBackgroundJobsApplicationContractsModule).Assembly,
            BackgroundJobsRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpBackgroundJobsHttpApiClientModule>();
        });
    }
}
