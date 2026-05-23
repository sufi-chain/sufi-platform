using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.TenantManagement;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppDomainSharedModule),
        typeof(SufiAbpAccountApplicationContractsModule),
        typeof(SufiAbpFeatureManagementApplicationContractsModule),
        typeof(SufiAbpIdentityApplicationContractsModule),
        typeof(SufiAbpPermissionManagementApplicationContractsModule),
        typeof(SufiAbpSettingManagementApplicationContractsModule),
        typeof(SufiAbpTenantManagementApplicationContractsModule),
        typeof(AbpObjectExtendingModule)
    )]
    public class DemoAppApplicationContractsModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            DemoAppDtoExtensions.Configure();
        }
    }
}
