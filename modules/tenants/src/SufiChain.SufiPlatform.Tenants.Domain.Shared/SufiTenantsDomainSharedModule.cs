using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Tenants.Localization;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiPlatform.Features;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Tenants;

[DependsOn(
    typeof(SufiUiDomainSharedModule),
    typeof(SufiFeaturesModule)
)]
public class SufiTenantsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiTenantsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiTenantsResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiPlatform.UI.Localization.SufiFrameworkResource))
                .AddVirtualJson("/Localization/Tenants");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.Tenants", typeof(SufiTenantsResource));
        });
    }
}
