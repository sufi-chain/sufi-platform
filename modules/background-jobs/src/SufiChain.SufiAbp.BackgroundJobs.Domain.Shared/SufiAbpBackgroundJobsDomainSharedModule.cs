using SufiChain.SufiAbp.BackgroundJobs.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiAbp.UI;

namespace SufiChain.SufiAbp.BackgroundJobs;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule)
)]
public class SufiAbpBackgroundJobsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpBackgroundJobsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpBackgroundJobsResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.UI.Localization.SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/BackgroundJobs");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.BackgroundJobs", typeof(SufiAbpBackgroundJobsResource));
        });
    }
}
