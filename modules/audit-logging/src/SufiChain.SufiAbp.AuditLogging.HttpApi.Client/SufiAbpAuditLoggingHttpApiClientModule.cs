using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.AuditLogging;

[DependsOn(
    typeof(SufiAbpAuditLoggingApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiAbpAuditLoggingHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpAuditLoggingApplicationContractsModule).Assembly,
            AuditLoggingRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpAuditLoggingHttpApiClientModule>();
        });
    }
}
