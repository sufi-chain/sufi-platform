using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.AspNetCore.Authentication;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.WebAssembly;

/// <summary>
/// ABP Module for Sufi WebAssembly authentication.
/// Provides token management and authentication handling for tiered Blazor WebAssembly apps.
/// </summary>
[DependsOn(
    typeof(AbpAutofacWebAssemblyModule)
)]
public class SufiAuthenticationWebAssemblyModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register authentication options
        context.Services.AddOptions<SufiAuthenticationOptions>();

        // Configure default authentication options for WebAssembly
        context.Services.Configure<SufiAuthenticationOptions>(options =>
        {
            // WebAssembly defaults are already set in SufiAuthenticationOptions
        });

        // Register access token provider
        context.Services.AddScoped<ISufiAccessTokenProvider, SufiAccessTokenProvider>();
    }
}
