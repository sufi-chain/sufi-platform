using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity.AspNetCore;

[DependsOn(
    typeof(SufiIdentityDomainModule),
    typeof(AbpSettingsModule)
)]
public class SufiIdentityAspNetCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IdentityBuilder>(builder =>
        {
            builder
                .AddDefaultTokenProviders()
                .AddSignInManager<SignInManager<IdentityUser>>();
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions(new SufiIdentityAspNetCoreOptions());

        if (options.ConfigureAuthentication)
        {
            context.Services
                .AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityConstants.ApplicationScheme;
                    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();
        }

        context.Services.ConfigureOptions<IdentityTokenOptionsConfigurator>();
    }
}
