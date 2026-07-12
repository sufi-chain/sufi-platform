using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// HTTP API client module for Tenants. Depends on ABP's
/// <see cref="SufiTenantsHttpApiClientModule"/> which registers a remote
/// <c>ITenantStore</c> — this is required for tiered Blazor hosts that have no
/// direct database access but need to resolve tenant names/IDs from cookies.
/// </summary>
[DependsOn(
    typeof(SufiTenantsApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class SufiTenantsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiTenantsApplicationContractsModule).Assembly,
            TenantsRemoteServiceConsts.RemoteServiceName
        );

        // Override ABP's TenantClientProxy to use Sufi routes (/api/tenants/tenants).
        // SufiTenantsHttpApiClientModule registers proxies for ABP's default routes,
        // but our Sufi TenantController serves ITenantAppService under the Sufi remote service name.
        context.Services.AddHttpClientProxies(
            typeof(SufiTenantsApplicationContractsModule).Assembly,
            TenantsRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiTenantsHttpApiClientModule>();
        });
    }
}
