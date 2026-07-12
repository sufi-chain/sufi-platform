using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Captcha.Turnstile;

[DependsOn(typeof(SufiCaptchaModule))]
public class SufiCaptchaTurnstileModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient(TurnstileCaptchaProvider.HttpClientName);
        context.Services.TryAddEnumerable(ServiceDescriptor.Transient<ICaptchaProvider, TurnstileCaptchaProvider>());
    }
}
