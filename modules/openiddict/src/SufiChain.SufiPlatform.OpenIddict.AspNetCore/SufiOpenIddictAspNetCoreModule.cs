using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp.Modularity;

using Volo.Abp.AspNetCore.MultiTenancy;
namespace SufiChain.SufiPlatform.OpenIddict;

[DependsOn(
    typeof(SufiOpenIddictDomainModule),
    typeof(AbpAspNetCoreMultiTenancyModule)
)]
public class SufiOpenIddictAspNetCoreModule : AbpModule
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
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        AddOpenIddictServer(context.Services, configuration, hostingEnvironment);

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

    private static void AddOpenIddictServer(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostingEnvironment)
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

                if (hostingEnvironment.IsDevelopment())
                {
                    builder
                        .AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    builder
                        .AddEncryptionCertificate(LoadCertificate(configuration, "Encryption"))
                        .AddSigningCertificate(LoadCertificate(configuration, "Signing"));
                }

                builder.DisableAccessTokenEncryption();

                services.ExecutePreConfiguredActions(builder);
            });

        services.ExecutePreConfiguredActions(openIddictBuilder);
    }

    private static X509Certificate2 LoadCertificate(
        IConfiguration configuration,
        string certificateName)
    {
        var configurationPath = $"OpenIddict:Certificates:{certificateName}";
        var base64 = configuration[$"{configurationPath}:Base64"];
        var password = configuration[$"{configurationPath}:Password"];

        if (string.IsNullOrWhiteSpace(base64) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                $"{configurationPath} must define protected Base64 and Password values outside Development.");
        }

        byte[] certificateBytes;
        try
        {
            certificateBytes = Convert.FromBase64String(base64);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                $"{configurationPath}:Base64 is not a valid PKCS#12 payload.",
                exception);
        }

        try
        {
            var certificate = X509CertificateLoader.LoadPkcs12(
                certificateBytes,
                password,
                X509KeyStorageFlags.EphemeralKeySet);

            if (!certificate.HasPrivateKey)
            {
                certificate.Dispose();
                throw new InvalidOperationException(
                    $"{configurationPath} must contain a private key.");
            }

            return certificate;
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"{configurationPath} could not be loaded as protected PKCS#12 certificate material.",
                exception);
        }
        finally
        {
            Array.Clear(certificateBytes);
        }
    }
}
