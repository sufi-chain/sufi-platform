using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Email;
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
        typeof(SufiAIDomainModule),
        typeof(SufiComModule)
    )]
    public class DemoAppDomainModule : AbpModule
    {
     
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpMultiTenancyOptions>(options =>
            {
                options.IsEnabled = MultiTenancyConsts.IsEnabled;
            });

#if DEBUG
            context.Services.Replace(ServiceDescriptor.Singleton<IEmailSender, NullEmailSender>());
#endif
        }
    }
}
