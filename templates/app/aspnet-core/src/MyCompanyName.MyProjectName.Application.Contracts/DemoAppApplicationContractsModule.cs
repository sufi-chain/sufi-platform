using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.ObjectExtending;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Modularity;

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
        typeof(SufiAbpObjectExtendingModule)
    )]
    public class DemoAppApplicationContractsModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            DemoAppDtoExtensions.Configure();
        }
    }
}
