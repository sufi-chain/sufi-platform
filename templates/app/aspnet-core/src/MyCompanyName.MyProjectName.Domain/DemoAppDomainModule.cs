using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.OpenIddict;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Permissions.Identity;
using SufiChain.SufiPlatform.Permissions.OpenIddict;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Users;
using MyCompanyName.MyProjectName.MultiTenancy;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppDomainSharedModule),
        typeof(SufiAuditLoggingDomainModule),
        typeof(SufiBackgroundJobsDomainModule),
        typeof(SufiFeaturesDomainModule),
        typeof(SufiIdentityDomainModule),
        typeof(SufiPermissionsDomainModule),
        typeof(SufiPermissionsDomainIdentityModule),
        typeof(SufiOpenIddictDomainModule),
        typeof(SufiPermissionsDomainOpenIddictModule),
        typeof(SufiSettingsDomainModule),
        typeof(SufiTenantsDomainModule),
        typeof(SufiUsersDomainModule),
        typeof(SufiAIDomainModule)
    )]
    public class DemoAppDomainModule : AbpModule
    {
     
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpMultiTenancyOptions>(options =>
            {
                options.IsEnabled = MultiTenancyConsts.IsEnabled;
            });
        }
    }
}
