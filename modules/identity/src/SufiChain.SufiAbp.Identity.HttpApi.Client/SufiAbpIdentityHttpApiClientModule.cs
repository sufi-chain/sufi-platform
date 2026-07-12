using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Identity;

[DependsOn(
    typeof(SufiAbpIdentityApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiAbpIdentityHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register proxies for SufiAbp-specific contracts (IOrganizationUnitAppService, IIdentitySecurityLogAppService, etc.)
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpIdentityApplicationContractsModule).Assembly,
            IdentityRemoteServiceConsts.RemoteServiceName
        );

        // Register proxies for ABP Identity contracts (IIdentityUserAppService, IIdentityRoleAppService, etc.)
        // These are served by our SufiAbp Identity HttpApi controllers under the SufiAbp remote service name.
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpIdentityApplicationContractsModule).Assembly,
            IdentityRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpIdentityHttpApiClientModule>();
        });
    }
}
