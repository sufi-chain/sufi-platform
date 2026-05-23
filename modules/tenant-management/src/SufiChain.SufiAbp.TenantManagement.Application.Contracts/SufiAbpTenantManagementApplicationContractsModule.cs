using Volo.Abp.Application;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.TenantManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpTenantManagementDomainSharedModule)
)]
public class SufiAbpTenantManagementApplicationContractsModule : AbpModule
{
}
