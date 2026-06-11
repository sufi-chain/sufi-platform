using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.JwtBearer;

/// <summary>
/// SufiAbp module for JWT bearer authentication.
/// Wraps ABP's JWT bearer authentication module for host applications.
/// </summary>
[DependsOn(typeof(AbpAspNetCoreAuthenticationJwtBearerModule))]
public class SufiAbpAspNetCoreAuthenticationJwtBearerModule : AbpModule
{
}
