using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Captcha.Recaptcha;

[DependsOn(typeof(SufiAbpCaptchaModule))]
public class SufiAbpCaptchaRecaptchaModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient(RecaptchaCaptchaProvider.HttpClientName);
        context.Services.TryAddEnumerable(ServiceDescriptor.Transient<ICaptchaProvider, RecaptchaCaptchaProvider>());
    }
}
