using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.LocalizationManagement;

[DependsOn(
    typeof(SufiAbpLocalizationManagementDomainSharedModule),
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule)
)]
public class SufiAbpLocalizationManagementApplicationContractsModule : AbpModule
{
}
