using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.LocalizationManagement;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpLocalizationManagementDomainSharedModule)
)]
public class SufiAbpLocalizationManagementDomainModule : AbpModule
{
}
