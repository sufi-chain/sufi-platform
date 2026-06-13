using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AuditLogging;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.OpenIddict;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.PermissionManagement.Identity;
using SufiChain.SufiAbp.PermissionManagement.OpenIddict;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.Users;
using SufiChain.SufiAbp.Messaging;
using SufiChain.SufiAbp.Messaging.Email;
using MyCompanyName.MyProjectName.MultiTenancy;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppDomainSharedModule),
        typeof(SufiAbpAuditLoggingDomainModule),
        typeof(SufiAbpBackgroundJobsDomainModule),
        typeof(SufiAbpFeatureManagementDomainModule),
        typeof(SufiAbpIdentityDomainModule),
        typeof(SufiAbpPermissionManagementDomainModule),
        typeof(SufiAbpPermissionManagementDomainIdentityModule),
        typeof(SufiAbpOpenIddictDomainModule),
        typeof(SufiAbpPermissionManagementDomainOpenIddictModule),
        typeof(SufiAbpSettingManagementDomainModule),
        typeof(SufiAbpTenantManagementDomainModule),
        typeof(SufiAbpUsersDomainModule),
        typeof(SufiAbpAIManagementDomainModule),
        typeof(SufiAbpMessagingModule)
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
