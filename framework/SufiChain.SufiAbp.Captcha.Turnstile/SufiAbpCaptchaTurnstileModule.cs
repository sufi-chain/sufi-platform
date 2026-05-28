using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Captcha.Turnstile;

[DependsOn(typeof(SufiAbpCaptchaModule))]
public class SufiAbpCaptchaTurnstileModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient(TurnstileCaptchaProvider.HttpClientName);
        context.Services.TryAddEnumerable(ServiceDescriptor.Transient<ICaptchaProvider, TurnstileCaptchaProvider>());
    }
}
