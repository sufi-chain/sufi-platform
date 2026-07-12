using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Captcha;

[DependsOn(
    typeof(AbpSettingsModule),
    typeof(AbpCachingModule)
)]
public class SufiAbpCaptchaModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddEnumerable(ServiceDescriptor.Transient<ICaptchaProvider, SimpleMathCaptchaProvider>());
        context.Services.TryAddEnumerable(ServiceDescriptor.Transient<ICaptchaProvider, NullCaptchaProvider>());

        context.Services.TryAddSingleton<CaptchaProviderRegistry>();
        context.Services.TryAddTransient<ICaptchaProviderResolver, CaptchaProviderResolver>();
        context.Services.TryAddTransient<ICaptchaValidator, CaptchaValidator>();
    }
}
