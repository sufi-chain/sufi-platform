using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Identity;

[DependsOn(
    typeof(SufiIdentityApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiIdentityHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register proxies for Sufi-specific contracts (IOrganizationUnitAppService, IIdentitySecurityLogAppService, etc.)
        context.Services.AddHttpClientProxies(
            typeof(SufiIdentityApplicationContractsModule).Assembly,
            IdentityRemoteServiceConsts.RemoteServiceName
        );

        // Register proxies for ABP Identity contracts (IIdentityUserAppService, IIdentityRoleAppService, etc.)
        // These are served by our Sufi Identity HttpApi controllers under the Sufi remote service name.
        context.Services.AddHttpClientProxies(
            typeof(SufiIdentityApplicationContractsModule).Assembly,
            IdentityRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiIdentityHttpApiClientModule>();
        });
    }
}
