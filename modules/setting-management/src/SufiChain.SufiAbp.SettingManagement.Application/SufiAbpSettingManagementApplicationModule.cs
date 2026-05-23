using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Emailing;

namespace SufiChain.SufiAbp.SettingManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpEmailingModule),
    typeof(SufiAbpSettingManagementDomainModule),
    typeof(SufiAbpSettingManagementApplicationContractsModule)
)]
public class SufiAbpSettingManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IEmailSettingsAppService, EmailSettingsAppService>();
        context.Services.AddTransient<ITimeZoneSettingsAppService, TimeZoneSettingsAppService>();
    }
}
