using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.TenantManagement;
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppDomainModule),
        typeof(SufiAbpAccountApplicationModule),
        typeof(DemoAppApplicationContractsModule),
        typeof(SufiAbpIdentityApplicationModule),
        typeof(SufiAbpPermissionManagementApplicationModule),
        typeof(SufiAbpTenantManagementApplicationModule),
        typeof(SufiAbpFeatureManagementApplicationModule),
        typeof(SufiAbpSettingManagementApplicationModule)
        )]
    public class DemoAppApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }
    }
}
