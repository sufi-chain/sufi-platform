using Volo.Abp.Domain;
using SufiChain.SufiAbp.Calendar.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.UI.Localization;
using SufiChain.SufiAbp.Validation;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiAbp.UI;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(AbpDddDomainSharedModule),
    typeof(SufiAbpValidationModule),
    typeof(SufiAbpFeaturesModule),
    typeof(SufiAbpUiDomainSharedModule)
)]
public class SufiAbpCalendarDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpCalendarDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<CalendarResource>("en")
                .AddBaseTypes(typeof(SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/Calendar");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Calendar", typeof(CalendarResource));
        });
    }
}
