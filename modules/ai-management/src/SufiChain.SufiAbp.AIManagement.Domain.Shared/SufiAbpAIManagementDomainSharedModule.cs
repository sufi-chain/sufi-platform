using SufiChain.SufiAbp.AIManagement.Localization;
using SufiChain.SufiAbp.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.AIManagement;

public class SufiAbpAIManagementDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpAIManagementDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<AIManagementResource>("en")
                .AddBaseTypes(typeof(SufiAbpValidationResource))
                .AddVirtualJson("/Localization/AIManagement");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("AIManagement", typeof(AIManagementResource));
        });
    }
}
