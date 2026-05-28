using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Messaging;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.SettingManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMessagingModule),
    typeof(SufiAbpSettingManagementDomainModule),
    typeof(SufiAbpSettingManagementApplicationContractsModule)
)]
public class SufiAbpSettingManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IEmailSettingsAppService, EmailSettingsAppService>();
        context.Services.AddTransient<ITimeZoneSettingsAppService, TimeZoneSettingsAppService>();
        context.Services.AddTransient<IIdentitySettingsAppService, IdentitySettingsAppService>();
    }
}
