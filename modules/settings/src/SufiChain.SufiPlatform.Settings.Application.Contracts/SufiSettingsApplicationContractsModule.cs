using Volo.Abp.Application;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.Settings;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiSettingsDomainSharedModule)
)]
public class SufiSettingsApplicationContractsModule : AbpModule
{
}
