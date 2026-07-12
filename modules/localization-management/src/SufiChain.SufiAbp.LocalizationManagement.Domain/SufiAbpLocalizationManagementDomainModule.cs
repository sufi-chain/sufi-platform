using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.LocalizationManagement;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiAbpLocalizationManagementDomainSharedModule)
)]
public class SufiAbpLocalizationManagementDomainModule : AbpModule
{
}
