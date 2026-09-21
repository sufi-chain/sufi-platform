using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.Identity.AspNetCore.ExternalAuth;

public class ExternalAuthOptionsConfigurator :
    IConfigureNamedOptions<GoogleOptions>,
    IConfigureNamedOptions<MicrosoftAccountOptions>,
    IConfigureNamedOptions<OAuthOptions>,
    ITransientDependency
{
    public const string GitHubScheme = "GitHub";

    private readonly ISettingProvider _settingProvider;
    private readonly IConfiguration _configuration;

    public ExternalAuthOptionsConfigurator(ISettingProvider settingProvider, IConfiguration configuration)
    {
        _settingProvider = settingProvider;
        _configuration = configuration;
    }

    public void Configure(GoogleOptions options) => Configure(GoogleDefaults.AuthenticationScheme, options);

    public void Configure(string? name, GoogleOptions options)
    {
        ApplyCredentials(
            options,
            IdentitySettingNames.ExternalAuth.Google.ClientId,
            IdentitySettingNames.ExternalAuth.Google.ClientSecret,
            "ExternalAuth:Google:ClientId",
            "ExternalAuth:Google:ClientSecret");
    }

    public void Configure(MicrosoftAccountOptions options) =>
        Configure(MicrosoftAccountDefaults.AuthenticationScheme, options);

    public void Configure(string? name, MicrosoftAccountOptions options)
    {
        ApplyCredentials(
            options,
            IdentitySettingNames.ExternalAuth.Microsoft.ClientId,
            IdentitySettingNames.ExternalAuth.Microsoft.ClientSecret,
            "ExternalAuth:Microsoft:ClientId",
            "ExternalAuth:Microsoft:ClientSecret");
    }

    public void Configure(OAuthOptions options)
    {
    }

    public void Configure(string? name, OAuthOptions options)
    {
        if (!string.Equals(name, GitHubScheme, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ApplyCredentials(
            options,
            IdentitySettingNames.ExternalAuth.GitHub.ClientId,
            IdentitySettingNames.ExternalAuth.GitHub.ClientSecret,
            "ExternalAuth:GitHub:ClientId",
            "ExternalAuth:GitHub:ClientSecret");
    }

    private void ApplyCredentials(
        OAuthOptions options,
        string settingClientId,
        string settingClientSecret,
        string configClientId,
        string configClientSecret)
    {
        var clientId = FirstNonEmpty(GetSetting(settingClientId), _configuration[configClientId]);
        var clientSecret = FirstNonEmpty(GetSetting(settingClientSecret), _configuration[configClientSecret]);

        if (!string.IsNullOrWhiteSpace(clientId))
        {
            options.ClientId = clientId;
        }

        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            options.ClientSecret = clientSecret;
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            options.ClientId = "unconfigured";
        }

        if (string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            options.ClientSecret = "unconfigured";
        }
    }

    private string? GetSetting(string name)
    {
        return AsyncHelper.RunSync(() => _settingProvider.GetOrNullAsync(name));
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    public static void ConfigureGitHubDefaults(OAuthOptions options)
    {
        options.CallbackPath = "/signin-github";
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";
        options.SaveTokens = true;
        options.Scope.Add("user:email");
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                context.RunClaimActions(payload.RootElement);
            }
        };
    }
}
