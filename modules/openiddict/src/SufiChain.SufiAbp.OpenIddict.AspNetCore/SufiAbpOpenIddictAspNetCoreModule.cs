using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp.Modularity;

using Volo.Abp.AspNetCore.MultiTenancy;
namespace SufiChain.SufiAbp.OpenIddict;

[DependsOn(
    typeof(SufiAbpOpenIddictDomainModule),
    typeof(AbpAspNetCoreMultiTenancyModule)
)]
public class SufiAbpOpenIddictAspNetCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<OpenIddictServerBuilder>(builder =>
        {
            builder.UseAspNetCore()
                .EnableAuthorizationEndpointPassthrough()
                .EnableTokenEndpointPassthrough()
                .EnableUserInfoEndpointPassthrough()
                .EnableEndSessionEndpointPassthrough()
                .DisableTransportSecurityRequirement();
        });

        PreConfigure<OpenIddictValidationBuilder>(builder =>
        {
            builder.UseAspNetCore();
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        AddOpenIddictServer(context.Services);

        Configure<AuthenticationOptions>(options =>
        {
            if (!options.SchemeMap.ContainsKey(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
            {
                options.AddScheme<OpenIddictServerAspNetCoreHandler>(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    displayName: null);
            }

            if (!options.SchemeMap.ContainsKey(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme))
            {
                options.AddScheme<OpenIddictValidationAspNetCoreHandler>(
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                    displayName: null);
            }
        });
    }

    private static void AddOpenIddictServer(IServiceCollection services)
    {
        var openIddictBuilder = services.AddOpenIddict()
            .AddServer(builder =>
            {
                builder
                    .SetAuthorizationEndpointUris("connect/authorize", "connect/authorize/callback")
                    .SetDeviceAuthorizationEndpointUris("device")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetEndSessionEndpointUris("connect/endsession")
                    .SetPushedAuthorizationEndpointUris("connect/par")
                    .SetRevocationEndpointUris("connect/revocat")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserInfoEndpointUris("connect/userinfo")
                    .SetEndUserVerificationEndpointUris("connect/verify");

                builder
                    .AllowAuthorizationCodeFlow()
                    .AllowHybridFlow()
                    .AllowImplicitFlow()
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow()
                    .AllowDeviceAuthorizationFlow()
                    .AllowNoneFlow()
                    .AllowTokenExchangeFlow();

                builder.RegisterScopes(
                    global::OpenIddict.Abstractions.OpenIddictConstants.Scopes.OpenId,
                    global::OpenIddict.Abstractions.OpenIddictConstants.Scopes.Email,
                    global::OpenIddict.Abstractions.OpenIddictConstants.Scopes.Profile,
                    global::OpenIddict.Abstractions.OpenIddictConstants.Scopes.Phone,
                    global::OpenIddict.Abstractions.OpenIddictConstants.Scopes.Roles,
                    global::OpenIddict.Abstractions.OpenIddictConstants.Scopes.Address,
                    global::OpenIddict.Abstractions.OpenIddictConstants.Scopes.OfflineAccess);

                builder.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableEndUserVerificationEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .DisableTransportSecurityRequirement();

                builder
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate()
                    .DisableAccessTokenEncryption();

                services.ExecutePreConfiguredActions(builder);
            });

        services.ExecutePreConfiguredActions(openIddictBuilder);
    }
}
