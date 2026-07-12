using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppDomainModule),
        typeof(SufiAccountApplicationModule),
        typeof(DemoAppApplicationContractsModule),
        typeof(SufiIdentityApplicationModule),
        typeof(SufiPermissionsApplicationModule),
        typeof(SufiTenantsApplicationModule),
        typeof(SufiFeaturesApplicationModule),
        typeof(SufiSettingsApplicationModule)
        )]
    public class DemoAppApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }
    }
}
