using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.TenantManagement;

/// <summary>
/// HTTP API client module for TenantManagement. Depends on ABP's
/// <see cref="SufiAbpTenantManagementHttpApiClientModule"/> which registers a remote
/// <c>ITenantStore</c> — this is required for tiered Blazor hosts that have no
/// direct database access but need to resolve tenant names/IDs from cookies.
/// </summary>
[DependsOn(
    typeof(SufiAbpTenantManagementApplicationContractsModule),
    typeof(SufiAbpHttpClientModule))]
public class SufiAbpTenantManagementHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpTenantManagementApplicationContractsModule).Assembly,
            TenantManagementRemoteServiceConsts.RemoteServiceName
        );

        // Override ABP's TenantClientProxy to use SufiAbp routes (/api/sabp/tenant-management/tenants).
        // SufiAbpTenantManagementHttpApiClientModule registers proxies for ABP's default routes,
        // but our SufiAbp TenantController serves ITenantAppService under the SufiAbp remote service name.
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpTenantManagementApplicationContractsModule).Assembly,
            TenantManagementRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpTenantManagementHttpApiClientModule>();
        });
    }
}
