using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Identity.AspNetCore;

[DependsOn(
    typeof(SufiAbpIdentityDomainModule)
)]
public class SufiAbpIdentityAspNetCoreModule : AbpModule
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
        var options = context.Services.ExecutePreConfiguredActions(new SufiAbpIdentityAspNetCoreOptions());

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
    }
}
