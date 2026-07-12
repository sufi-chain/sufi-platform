using SufiChain.SufiPlatform.BackgroundJobs.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.BackgroundJobs;

[DependsOn(
    typeof(SufiUiDomainSharedModule)
)]
public class SufiBackgroundJobsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiBackgroundJobsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiBackgroundJobsResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiPlatform.UI.Localization.SufiFrameworkResource))
                .AddVirtualJson("/Localization/BackgroundJobs");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.BackgroundJobs", typeof(SufiBackgroundJobsResource));
        });
    }
}
