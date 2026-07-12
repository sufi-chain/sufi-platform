using MyCompanyName.MyProjectName.Localization;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.OpenIddict;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.Tags;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Menus;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiPlatform.UI.Localization;

using SufiChain.SufiPlatform.UI;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(SufiUiDomainSharedModule),
        typeof(SufiAuditLoggingDomainSharedModule),
        typeof(SufiBackgroundJobsDomainSharedModule),
        typeof(SufiFeaturesDomainSharedModule),
        typeof(SufiIdentityDomainSharedModule),
        typeof(SufiOpenIddictDomainSharedModule),
        typeof(SufiPermissionsDomainSharedModule),
        typeof(SufiSettingsDomainSharedModule),
        typeof(SufiTenantsDomainSharedModule),
        typeof(SufiShortLinksDomainSharedModule),
        typeof(SufiTagsDomainSharedModule),
        typeof(SufiMenusDomainSharedModule)
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
                    .AddBaseTypes(typeof(SufiFrameworkResource))
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