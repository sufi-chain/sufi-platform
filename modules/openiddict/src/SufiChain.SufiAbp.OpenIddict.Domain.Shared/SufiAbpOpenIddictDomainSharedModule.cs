using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Validation;
using SufiChain.SufiAbp.Validation.Localization;
using SufiChain.SufiAbp.OpenIddict.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiAbp.UI;

namespace SufiChain.SufiAbp.OpenIddict;

[DependsOn(
    typeof(SufiAbpValidationModule),
    typeof(AbpVirtualFileSystemModule),
    typeof(SufiAbpUiDomainSharedModule)
)]
public class SufiAbpOpenIddictDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpOpenIddictDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpOpenIddictResource>("en")
                .AddBaseTypes(typeof(SufiAbpValidationResource))
                .AddVirtualJson("/Localization/SufiAbpOpenIddict");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("OpenIddict", typeof(SufiAbpOpenIddictResource));
        });
    }
}
