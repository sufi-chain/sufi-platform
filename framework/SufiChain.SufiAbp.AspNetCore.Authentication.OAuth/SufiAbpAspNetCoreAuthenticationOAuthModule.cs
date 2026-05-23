using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Authentication.OAuth;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.OAuth;

/// <summary>
/// SufiAbp module for OAuth external logins (Google, Microsoft, Facebook).
/// Wraps ABP's OAuth module which provides MapAbpClaimTypes for claim mapping.
/// Hosts configure AddGoogle, AddMicrosoft, AddFacebook and call options.ClaimActions.MapAbpClaimTypes().
/// </summary>
[DependsOn(typeof(AbpAspNetCoreAuthenticationOAuthModule))]
public class SufiAbpAspNetCoreAuthenticationOAuthModule : AbpModule
{
}
