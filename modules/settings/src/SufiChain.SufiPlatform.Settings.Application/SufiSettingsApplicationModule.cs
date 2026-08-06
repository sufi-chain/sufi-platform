using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Ddd;
using SufiChain.SufiPlatform.SufiCom;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Settings;

[DependsOn(
    typeof(SufiDddApplicationModule),
    typeof(SufiComModule),
    typeof(SufiSettingsDomainModule),
    typeof(SufiSettingsApplicationContractsModule)
)]
public class SufiSettingsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IEmailSettingsAppService, EmailSettingsAppService>();
        context.Services.AddTransient<ITimeZoneSettingsAppService, TimeZoneSettingsAppService>();
        context.Services.AddTransient<IIdentitySettingsAppService, IdentitySettingsAppService>();
    }
}
