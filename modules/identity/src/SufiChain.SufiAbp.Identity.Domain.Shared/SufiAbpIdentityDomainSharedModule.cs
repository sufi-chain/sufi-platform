using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiAbp.UI;

namespace SufiChain.SufiAbp.Identity;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule)
)]
public class SufiAbpIdentityDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpIdentityDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                 .Add<SufiAbpIdentityResource>("en")
                 .AddBaseTypes(
                     typeof(SufiAbpFrameworkResource)
                 )
                 .AddVirtualJson("/Localization/Identity");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.Identity", typeof(SufiAbpIdentityResource));
        });
    }
}
