using SufiChain.SufiAbp.AspNetCore.Mvc;
using SufiChain.SufiAbp.Calendar.Localization;
using SufiChain.SufiAbp.UI.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(SufiAbpCalendarApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAbpCalendarHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(SufiAbpCalendarApplicationContractsModule).Assembly);
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<CalendarResource>()
                .AddBaseTypes(typeof(SufiAbpUiResource));
        });
    }
}
