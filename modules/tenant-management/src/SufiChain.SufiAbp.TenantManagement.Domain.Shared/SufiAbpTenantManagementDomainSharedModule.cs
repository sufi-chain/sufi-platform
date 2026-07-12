using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.TenantManagement.Localization;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiAbp.Features;

using SufiChain.SufiAbp.UI;

namespace SufiChain.SufiAbp.TenantManagement;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule),
    typeof(SufiAbpFeaturesModule)
)]
public class SufiAbpTenantManagementDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpTenantManagementDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpTenantManagementResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.UI.Localization.SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/TenantManagement");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.TenantManagement", typeof(SufiAbpTenantManagementResource));
        });
    }
}
