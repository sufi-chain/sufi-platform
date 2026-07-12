using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Identity;

[DependsOn(
    typeof(SufiUiDomainSharedModule)
)]
public class SufiIdentityDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiIdentityDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                 .Add<SufiIdentityResource>("en")
                 .AddBaseTypes(
                     typeof(SufiFrameworkResource)
                 )
                 .AddVirtualJson("/Localization/Identity");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.Identity", typeof(SufiIdentityResource));
        });
    }
}
