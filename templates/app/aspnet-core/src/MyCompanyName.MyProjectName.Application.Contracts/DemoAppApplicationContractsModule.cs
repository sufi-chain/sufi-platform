using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppDomainSharedModule),
        typeof(SufiAccountApplicationContractsModule),
        typeof(SufiFeaturesApplicationContractsModule),
        typeof(SufiIdentityApplicationContractsModule),
        typeof(SufiPermissionsApplicationContractsModule),
        typeof(SufiSettingsApplicationContractsModule),
        typeof(SufiTenantsApplicationContractsModule),
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
