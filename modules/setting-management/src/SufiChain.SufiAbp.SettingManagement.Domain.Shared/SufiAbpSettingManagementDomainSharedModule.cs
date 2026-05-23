using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using SufiChain.SufiAbp.SettingManagement.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.SettingManagement.Localization;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiAbp.UI;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.SettingManagement;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule),
    typeof(SufiAbpFeaturesModule)
)]
public class SufiAbpSettingManagementDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpSettingManagementDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpSettingManagementResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.UI.Localization.SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/SettingManagement");

            options.Resources
                .Get<SufiAbpSettingManagementResource>()
                .AddVirtualJson("/Localization/SufiAbpSettingManagement");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.SettingManagement", typeof(SufiAbpSettingManagementResource));
        });
    }
}
