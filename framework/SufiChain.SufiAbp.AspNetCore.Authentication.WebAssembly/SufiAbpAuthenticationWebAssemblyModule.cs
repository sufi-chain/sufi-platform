using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Authentication;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.WebAssembly;

/// <summary>
/// ABP Module for SufiAbp WebAssembly authentication.
/// Provides token management and authentication handling for tiered Blazor WebAssembly apps.
/// </summary>
[DependsOn(
    typeof(AbpAutofacWebAssemblyModule)
)]
public class SufiAbpAuthenticationWebAssemblyModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register authentication options
        context.Services.AddOptions<SufiAbpAuthenticationOptions>();

        // Configure default authentication options for WebAssembly
        context.Services.Configure<SufiAbpAuthenticationOptions>(options =>
        {
            // WebAssembly defaults are already set in SufiAbpAuthenticationOptions
        });

        // Register access token provider
        context.Services.AddScoped<ISufiAbpAccessTokenProvider, SufiAbpAccessTokenProvider>();
    }
}
