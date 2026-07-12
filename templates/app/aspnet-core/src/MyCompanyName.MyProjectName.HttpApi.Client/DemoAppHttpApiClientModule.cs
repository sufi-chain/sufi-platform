using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Http.Client;
namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppApplicationContractsModule),
        typeof(AbpHttpClientModule)
    )]
    public class DemoAppHttpApiClientModule : AbpModule
    {
        public const string RemoteServiceName = "Default";

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddHttpClientProxies(
                typeof(DemoAppApplicationContractsModule).Assembly,
                RemoteServiceName
            );

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<DemoAppHttpApiClientModule>();
            });
        }
    }
}
