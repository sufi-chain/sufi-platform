using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using OidcConstants = OpenIddict.Abstractions.OpenIddictConstants;
using SufiChain.SufiAbp.OpenIddict.Applications;
using SufiChain.SufiAbp.OpenIddict.Scopes;
using SufiChain.SufiAbp.PermissionManagement;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace MyCompanyName.MyProjectName.OpenIddict;

/* Creates initial data that is needed to properly run the application
 * and make client-to-server communication possible.
 *
 * Aligns with SufiChain.SufiAbp framework login flows (SufiAbpAuthenticationOptions, AccountController):
 * - OIDC clients (MVC, Blazor Server): signin-oidc, signout-callback-oidc
 *
 * Only seeds clients when both ClientId and RootUrl are configured (DbMigrator
 * may run with minimal config lacking RootUrl). Skipped clients are seeded when
 * hosts run with full appsettings.
 */
public class OpenIddictDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IOpenIddictApplicationRepository _applicationRepository;
    private readonly IOpenIddictScopeRepository _scopeRepository;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IStringLocalizer<OpenIddictResponse> L;

    public OpenIddictDataSeedContributor(
        IConfiguration configuration,
        IGuidGenerator guidGenerator,
        IOpenIddictApplicationRepository applicationRepository,
        IOpenIddictScopeRepository scopeRepository,
        IPermissionDataSeeder permissionDataSeeder,
        IStringLocalizer<OpenIddictResponse> l)
    {
        _configuration = configuration;
        _guidGenerator = guidGenerator;
        _applicationRepository = applicationRepository;
        _scopeRepository = scopeRepository;
        _permissionDataSeeder = permissionDataSeeder;
        L = l;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateScopesAsync();
        await CreateApplicationsAsync();
    }

    private async Task CreateScopesAsync()
    {
        if (await _scopeRepository.FindByNameAsync("DemoApp") == null)
        {
            await _scopeRepository.InsertAsync(new OpenIddictScope(_guidGenerator.Create())
            {
                Description = "DemoApp API",
                Name = "DemoApp",
                DisplayName = "DemoApp API",
                Resources = JsonSerializer.Serialize(new[] { "DemoApp" }),
                Descriptions = JsonSerializer.Serialize(new Dictionary<string, string>()),
                DisplayNames = JsonSerializer.Serialize(new Dictionary<string, string>()),
                Properties = JsonSerializer.Serialize(new Dictionary<string, string>())
            }, autoSave: true);
        }
    }

    private async Task CreateApplicationsAsync()
    {
        var commonScopes = new List<string>
        {
            OidcConstants.Permissions.Scopes.Address,
            OidcConstants.Permissions.Scopes.Email,
            OidcConstants.Permissions.Scopes.Phone,
            OidcConstants.Permissions.Scopes.Profile,
            OidcConstants.Permissions.Scopes.Roles,
            "DemoApp"
        };

        var configurationSection = _configuration.GetSection("OpenIddict:Applications");

        //Web Client
        var webClientId = configurationSection["DemoApp_Web:ClientId"];
        var webClientRootUrl = configurationSection["DemoApp_Web:RootUrl"];
        if (!webClientId.IsNullOrWhiteSpace() && !webClientRootUrl.IsNullOrWhiteSpace())
        {
            webClientRootUrl = webClientRootUrl.EnsureEndsWith('/');

            /* DemoApp_Web client is only needed if you created a tiered
             * solution. Otherwise, you can delete this client. */
            await CreateApplicationAsync(
                name: webClientId!,
                type: OidcConstants.ClientTypes.Confidential,
                consentType: OidcConstants.ConsentTypes.Implicit,
                displayName: "Web Application",
                secret: configurationSection["DemoApp_Web:ClientSecret"] ?? "1q2w3e*",
                grantTypes: new List<string> //Hybrid flow
                {
                    OidcConstants.GrantTypes.AuthorizationCode,
                    OidcConstants.GrantTypes.Implicit
                },
                scopes: commonScopes,
                redirectUri: $"{webClientRootUrl}signin-oidc",
                clientUri: webClientRootUrl,
                postLogoutRedirectUri: $"{webClientRootUrl}signout-callback-oidc"
            );
        }

        //Console Test / Angular Client
        var consoleAndAngularClientId = configurationSection["DemoApp_App:ClientId"];
        if (!consoleAndAngularClientId.IsNullOrWhiteSpace())
        {
            var consoleAndAngularClientRootUrl = configurationSection["DemoApp_App:RootUrl"]?.TrimEnd('/');
            await CreateApplicationAsync(
                name: consoleAndAngularClientId!,
                type: OidcConstants.ClientTypes.Public,
                consentType: OidcConstants.ConsentTypes.Implicit,
                displayName: "Console Test / Angular Application",
                secret: null,
                grantTypes: new List<string>
                {
                    OidcConstants.GrantTypes.AuthorizationCode,
                    OidcConstants.GrantTypes.Password,
                    OidcConstants.GrantTypes.ClientCredentials,
                    OidcConstants.GrantTypes.RefreshToken
                },
                scopes: commonScopes,
                redirectUri: consoleAndAngularClientRootUrl,
                clientUri: consoleAndAngularClientRootUrl,
                postLogoutRedirectUri: consoleAndAngularClientRootUrl
            );
        }

        // Blazor Client (uses AddOpenIdConnect with signin-oidc / signout-callback-oidc)
        var blazorClientId = configurationSection["DemoApp_Blazor:ClientId"];
        var blazorRootUrl = configurationSection["DemoApp_Blazor:RootUrl"];
        if (!blazorClientId.IsNullOrWhiteSpace() && !blazorRootUrl.IsNullOrWhiteSpace())
        {
            blazorRootUrl = blazorRootUrl.EnsureEndsWith('/');

            await CreateApplicationAsync(
                name: blazorClientId!,
                type: OidcConstants.ClientTypes.Confidential,
                consentType: OidcConstants.ConsentTypes.Implicit,
                displayName: "Blazor Application",
                secret: configurationSection["DemoApp_Blazor:ClientSecret"] ?? "1q2w3e*",
                grantTypes: new List<string>
                {
                    OidcConstants.GrantTypes.AuthorizationCode,
                    OidcConstants.GrantTypes.Implicit
                },
                scopes: commonScopes,
                redirectUri: $"{blazorRootUrl}signin-oidc",
                clientUri: blazorRootUrl.TrimEnd('/'),
                postLogoutRedirectUri: $"{blazorRootUrl}signout-callback-oidc"
            );
        }

        // Blazor Server Tiered Client
        var blazorServerTieredClientId = configurationSection["DemoApp_BlazorServerTiered:ClientId"];
        var blazorServerTieredRootUrl = configurationSection["DemoApp_BlazorServerTiered:RootUrl"];
        if (!blazorServerTieredClientId.IsNullOrWhiteSpace() && !blazorServerTieredRootUrl.IsNullOrWhiteSpace())
        {
            blazorServerTieredRootUrl = blazorServerTieredRootUrl.EnsureEndsWith('/');

            await CreateApplicationAsync(
                name: blazorServerTieredClientId!,
                type: OidcConstants.ClientTypes.Confidential,
                consentType: OidcConstants.ConsentTypes.Implicit,
                displayName: "Blazor Server Application",
                secret: configurationSection["DemoApp_BlazorServerTiered:ClientSecret"] ?? "1q2w3e*",
                grantTypes: new List<string> //Hybrid flow
                {
                    OidcConstants.GrantTypes.AuthorizationCode,
                    OidcConstants.GrantTypes.Implicit
                },
                scopes: commonScopes,
                redirectUri: $"{blazorServerTieredRootUrl}signin-oidc",
                clientUri: blazorServerTieredRootUrl,
                postLogoutRedirectUri: $"{blazorServerTieredRootUrl}signout-callback-oidc"
            );
        }

        // <TEMPLATE-REMOVE IF-NOT="host:website">
        // Blazor WebSite Client (Public-facing website for CMS)
        var blazorWebSiteClientId = configurationSection["DemoApp_BlazorWebSite:ClientId"];
        var blazorWebSiteRootUrl = configurationSection["DemoApp_BlazorWebSite:RootUrl"];
        if (!blazorWebSiteClientId.IsNullOrWhiteSpace() && !blazorWebSiteRootUrl.IsNullOrWhiteSpace())
        {
            blazorWebSiteRootUrl = blazorWebSiteRootUrl.EnsureEndsWith('/');

            await CreateApplicationAsync(
                name: blazorWebSiteClientId!,
                type: OidcConstants.ClientTypes.Confidential,
                consentType: OidcConstants.ConsentTypes.Implicit,
                displayName: "Blazor WebSite Application",
                secret: configurationSection["DemoApp_BlazorWebSite:ClientSecret"] ?? "1q2w3e*",
                grantTypes: new List<string> //Hybrid flow
                {
                    OidcConstants.GrantTypes.AuthorizationCode,
                    OidcConstants.GrantTypes.Implicit
                },
                scopes: commonScopes,
                redirectUri: $"{blazorWebSiteRootUrl}signin-oidc",
                clientUri: blazorWebSiteRootUrl,
                postLogoutRedirectUri: $"{blazorWebSiteRootUrl}signout-callback-oidc"
            );
        }
        // </TEMPLATE-REMOVE>

        // Swagger Client
        var swaggerClientId = configurationSection["DemoApp_Swagger:ClientId"];
        var swaggerRootUrl = configurationSection["DemoApp_Swagger:RootUrl"]?.TrimEnd('/');
        if (!swaggerClientId.IsNullOrWhiteSpace() && !swaggerRootUrl.IsNullOrWhiteSpace())
        {
            await CreateApplicationAsync(
                name: swaggerClientId!,
                type: OidcConstants.ClientTypes.Public,
                consentType: OidcConstants.ConsentTypes.Implicit,
                displayName: "Swagger Application",
                secret: null,
                grantTypes: new List<string>
                {
                    OidcConstants.GrantTypes.AuthorizationCode,
                },
                scopes: commonScopes,
                redirectUri: $"{swaggerRootUrl}/swagger/oauth2-redirect.html",
                clientUri: swaggerRootUrl
            );
        }
    }

    private async Task CreateApplicationAsync(
        [NotNull] string name,
        [NotNull] string type,
        [NotNull] string consentType,
        string displayName,
        string? secret,
        List<string> grantTypes,
        List<string> scopes,
        string? clientUri = null,
        string? redirectUri = null,
        string? postLogoutRedirectUri = null,
        List<string>? permissions = null)
    {
        if (!string.IsNullOrEmpty(secret) && string.Equals(type, OidcConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["NoClientSecretCanBeSetForPublicApplications"]);
        }

        if (string.IsNullOrEmpty(secret) && string.Equals(type, OidcConstants.ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["TheClientSecretIsRequiredForConfidentialApplications"]);
        }

        if (!string.IsNullOrEmpty(name) && await _applicationRepository.FindByClientIdAsync(name) != null)
        {
            return;
            //throw new BusinessException(L["TheClientIdentifierIsAlreadyTakenByAnotherApplication"]);
        }

        var client = await _applicationRepository.FindByClientIdAsync(name);
        if (client == null)
        {
            var application = new OpenIddictApplicationDescriptor
            {
                ClientId = name,
                ClientType = type,
                ClientSecret = secret,
                ConsentType = consentType,
                DisplayName = displayName,
            };

            Check.NotNullOrEmpty(grantTypes, nameof(grantTypes));
            Check.NotNullOrEmpty(scopes, nameof(scopes));

            if (new [] { OidcConstants.GrantTypes.AuthorizationCode, OidcConstants.GrantTypes.Implicit }.All(grantTypes.Contains))
            {
                application.Permissions.Add(OidcConstants.Permissions.ResponseTypes.CodeIdToken);

                if (string.Equals(type, OidcConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    application.Permissions.Add(OidcConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                    application.Permissions.Add(OidcConstants.Permissions.ResponseTypes.CodeToken);
                }
            }

            if (!redirectUri.IsNullOrWhiteSpace() || !postLogoutRedirectUri.IsNullOrWhiteSpace())
            {
                application.Permissions.Add(OidcConstants.Permissions.Endpoints.EndSession);
            }

            var buildInGrantTypes = new []
            {
                OidcConstants.GrantTypes.Implicit,
                OidcConstants.GrantTypes.Password,
                OidcConstants.GrantTypes.AuthorizationCode,
                OidcConstants.GrantTypes.ClientCredentials,
                OidcConstants.GrantTypes.DeviceCode,
                OidcConstants.GrantTypes.RefreshToken
            };

            foreach (var grantType in grantTypes)
            {
              if (grantType == OidcConstants.GrantTypes.AuthorizationCode)
              {
                  application.Permissions.Add(OidcConstants.Permissions.GrantTypes.AuthorizationCode);
                  application.Permissions.Add(OidcConstants.Permissions.ResponseTypes.Code);
              }

              if (grantType == OidcConstants.GrantTypes.AuthorizationCode || grantType == OidcConstants.GrantTypes.Implicit)
              {
                  application.Permissions.Add(OidcConstants.Permissions.Endpoints.Authorization);
              }

              if (grantType == OidcConstants.GrantTypes.AuthorizationCode ||
                  grantType == OidcConstants.GrantTypes.ClientCredentials ||
                  grantType == OidcConstants.GrantTypes.Password ||
                  grantType == OidcConstants.GrantTypes.RefreshToken ||
                  grantType == OidcConstants.GrantTypes.DeviceCode)
              {
                  application.Permissions.Add(OidcConstants.Permissions.Endpoints.Token);
                  application.Permissions.Add(OidcConstants.Permissions.Endpoints.Revocation);
                  application.Permissions.Add(OidcConstants.Permissions.Endpoints.Introspection);
              }

              if (grantType == OidcConstants.GrantTypes.ClientCredentials)
              {
                  application.Permissions.Add(OidcConstants.Permissions.GrantTypes.ClientCredentials);
              }

              if (grantType == OidcConstants.GrantTypes.Implicit)
              {
                  application.Permissions.Add(OidcConstants.Permissions.GrantTypes.Implicit);
              }

              if (grantType == OidcConstants.GrantTypes.Password)
              {
                  application.Permissions.Add(OidcConstants.Permissions.GrantTypes.Password);
              }

              if (grantType == OidcConstants.GrantTypes.RefreshToken)
              {
                  application.Permissions.Add(OidcConstants.Permissions.GrantTypes.RefreshToken);
              }

              if (grantType == OidcConstants.GrantTypes.DeviceCode)
              {
                  application.Permissions.Add(OidcConstants.Permissions.GrantTypes.DeviceCode);
                  application.Permissions.Add(OidcConstants.Permissions.Endpoints.DeviceAuthorization);
              }

              if (grantType == OidcConstants.GrantTypes.Implicit)
              {
                  application.Permissions.Add(OidcConstants.Permissions.ResponseTypes.IdToken);
                  if (string.Equals(type, OidcConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                  {
                      application.Permissions.Add(OidcConstants.Permissions.ResponseTypes.IdTokenToken);
                      application.Permissions.Add(OidcConstants.Permissions.ResponseTypes.Token);
                  }
              }

              if (!buildInGrantTypes.Contains(grantType))
              {
                  application.Permissions.Add(OidcConstants.Permissions.Prefixes.GrantType + grantType);
              }
            }

            var buildInScopes = new []
            {
                OidcConstants.Permissions.Scopes.Address,
                OidcConstants.Permissions.Scopes.Email,
                OidcConstants.Permissions.Scopes.Phone,
                OidcConstants.Permissions.Scopes.Profile,
                OidcConstants.Permissions.Scopes.Roles
            };

            foreach (var scope in scopes)
            {
                if (buildInScopes.Contains(scope))
                {
                    application.Permissions.Add(scope);
                }
                else
                {
                    application.Permissions.Add(OidcConstants.Permissions.Prefixes.Scope + scope);
                }
            }

            if (redirectUri != null)
            {
                if (!redirectUri.IsNullOrEmpty())
                {
                    if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                    {
                        throw new BusinessException(L["InvalidRedirectUri", redirectUri]);
                    }

                    if (application.RedirectUris.All(x => x != uri))
                    {
                        application.RedirectUris.Add(uri);
                    }
                }
            }

            if (postLogoutRedirectUri != null)
            {
                if (!postLogoutRedirectUri.IsNullOrEmpty())
                {
                    if (!Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                    {
                        throw new BusinessException(L["InvalidPostLogoutRedirectUri", postLogoutRedirectUri]);
                    }

                    if (application.PostLogoutRedirectUris.All(x => x != uri))
                    {
                        application.PostLogoutRedirectUris.Add(uri);
                    }
                }
            }

            if (permissions != null)
            {
                await _permissionDataSeeder.SeedAsync(
                    ClientPermissionValueProvider.ProviderName,
                    name,
                    permissions,
                    null
                );
            }

            await _applicationRepository.InsertAsync(new OpenIddictApplication(_guidGenerator.Create())
            {
                ApplicationType = "web",
                ClientId = application.ClientId ?? string.Empty,
                ClientSecret = application.ClientSecret ?? string.Empty,
                ClientType = application.ClientType ?? string.Empty,
                ConsentType = application.ConsentType ?? string.Empty,
                DisplayName = application.DisplayName ?? string.Empty,
                DisplayNames = JsonSerializer.Serialize(application.DisplayNames),
                JsonWebKeySet = application.JsonWebKeySet is null
                    ? "{}"
                    : JsonSerializer.Serialize(application.JsonWebKeySet),
                Permissions = JsonSerializer.Serialize(application.Permissions),
                PostLogoutRedirectUris = JsonSerializer.Serialize(application.PostLogoutRedirectUris.Select(uri => uri.AbsoluteUri)),
                Properties = JsonSerializer.Serialize(application.Properties),
                RedirectUris = JsonSerializer.Serialize(application.RedirectUris.Select(uri => uri.AbsoluteUri)),
                Requirements = JsonSerializer.Serialize(application.Requirements),
                Settings = JsonSerializer.Serialize(application.Settings),
                FrontChannelLogoutUri = string.Empty,
                ClientUri = clientUri ?? string.Empty,
                LogoUri = string.Empty
            }, autoSave: true);
        }
    }
}
