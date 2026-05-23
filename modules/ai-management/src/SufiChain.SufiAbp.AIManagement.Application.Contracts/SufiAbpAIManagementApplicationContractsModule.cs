using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
)]
public class SufiAbpAIManagementApplicationContractsModule : AbpModule
{
}
