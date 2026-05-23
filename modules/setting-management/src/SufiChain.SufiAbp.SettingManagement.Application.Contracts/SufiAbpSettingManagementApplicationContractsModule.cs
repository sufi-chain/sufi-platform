using Volo.Abp.Application;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.SettingManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpSettingManagementDomainSharedModule)
)]
public class SufiAbpSettingManagementApplicationContractsModule : AbpModule
{
}
