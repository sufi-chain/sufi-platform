using SufiChain.SufiPlatform.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.UI.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(SufiCalendarApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiCalendarHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(SufiCalendarApplicationContractsModule).Assembly);
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<CalendarResource>()
                .AddBaseTypes(typeof(SufiUiResource));
        });
    }
}