using Volo.Abp.Domain;
using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.Validation;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(AbpDddDomainSharedModule),
    typeof(SufiValidationModule),
    typeof(SufiFeaturesModule),
    typeof(SufiUiDomainSharedModule)
)]
public class SufiCalendarDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiCalendarDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<CalendarResource>("en")
                .AddBaseTypes(typeof(SufiFrameworkResource))
                .AddVirtualJson("/Localization/Calendar");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Calendar", typeof(CalendarResource));
        });
    }
}