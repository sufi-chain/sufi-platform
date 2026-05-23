using SufiChain.SufiAbp.UI;
using MyCompanyName.MyProjectName.Localization;
using SufiChain.SufiAbp.AuditLogging;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.OpenIddict;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.TenantManagement;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiAbp.UI.Localization;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(SufiAbpUiDomainSharedModule),
        typeof(SufiAbpAuditLoggingDomainSharedModule),
        typeof(SufiAbpBackgroundJobsDomainSharedModule),
        typeof(SufiAbpFeatureManagementDomainSharedModule),
        typeof(SufiAbpIdentityDomainSharedModule),
        typeof(SufiAbpOpenIddictDomainSharedModule),
        typeof(SufiAbpPermissionManagementDomainSharedModule),
        typeof(SufiAbpSettingManagementDomainSharedModule),
        typeof(SufiAbpTenantManagementDomainSharedModule)
        )]
    public class DemoAppDomainSharedModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            DemoAppGlobalFeatureConfigurator.Configure();
            DemoAppModuleExtensionConfigurator.Configure();
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<DemoAppDomainSharedModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<DemoAppResource>("en")
                    .AddBaseTypes(typeof(SufiAbpFrameworkResource))
                    .AddVirtualJson("/Localization/DemoApp");

                options.DefaultResourceType = typeof(DemoAppResource);
            });

            Configure<AbpExceptionLocalizationOptions>(options =>
            {
                options.MapCodeNamespace("DemoApp", typeof(DemoAppResource));
            });
        }
    }
}
