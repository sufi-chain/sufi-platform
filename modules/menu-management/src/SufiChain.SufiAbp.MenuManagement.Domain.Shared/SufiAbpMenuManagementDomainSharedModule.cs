using SufiChain.SufiAbp.MenuManagement.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.Validation;
using SufiChain.SufiAbp.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(
    typeof(SufiAbpValidationModule),
    typeof(SufiAbpFeaturesModule))]
public class SufiAbpMenuManagementDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options => options.FileSets.AddEmbedded<SufiAbpMenuManagementDomainSharedModule>());
        Configure<AbpLocalizationOptions>(options => options.Resources.Add<SufiAbpMenuManagementResource>("en").AddBaseTypes(typeof(SufiAbpValidationResource)).AddVirtualJson("/Localization/MenuManagement"));
        Configure<AbpExceptionLocalizationOptions>(options => options.MapCodeNamespace(MenuManagementErrorCodes.Namespace, typeof(SufiAbpMenuManagementResource)));
    }
}
