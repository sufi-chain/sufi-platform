using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using SufiChain.SufiPlatform.Settings.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.Localization;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiPlatform.Features;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Settings;

[DependsOn(
    typeof(SufiUiDomainSharedModule),
    typeof(SufiFeaturesModule)
)]
public class SufiSettingsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiSettingsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiSettingsResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiPlatform.UI.Localization.SufiFrameworkResource))
                .AddVirtualJson("/Localization/Settings");

            options.Resources
                .Get<SufiSettingsResource>()
                .AddVirtualJson("/Localization/SufiSettings");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.Settings", typeof(SufiSettingsResource));
        });
    }
}
