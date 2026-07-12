using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Captcha.Recaptcha;

[DependsOn(typeof(SufiCaptchaModule))]
public class SufiCaptchaRecaptchaModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient(RecaptchaCaptchaProvider.HttpClientName);
        context.Services.TryAddEnumerable(ServiceDescriptor.Transient<ICaptchaProvider, RecaptchaCaptchaProvider>());
    }
}
