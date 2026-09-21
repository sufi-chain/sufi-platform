using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SufiChain.SufiPlatform.Identity.AspNetCore.ExternalAuth;

public static class ExternalAuthAuthenticationExtensions
{
    public static AuthenticationBuilder AddSufiExternalAuthProviders(
        this AuthenticationBuilder authBuilder,
        IConfiguration configuration)
    {
        authBuilder.AddGoogle(options =>
        {
            options.ClientId = CredentialOrPlaceholder(configuration["ExternalAuth:Google:ClientId"]);
            options.ClientSecret = CredentialOrPlaceholder(configuration["ExternalAuth:Google:ClientSecret"]);
        });

        authBuilder.AddMicrosoftAccount(options =>
        {
            options.ClientId = CredentialOrPlaceholder(configuration["ExternalAuth:Microsoft:ClientId"]);
            options.ClientSecret = CredentialOrPlaceholder(configuration["ExternalAuth:Microsoft:ClientSecret"]);
        });

        authBuilder.AddOAuth(ExternalAuthOptionsConfigurator.GitHubScheme, "GitHub", options =>
        {
            options.ClientId = CredentialOrPlaceholder(configuration["ExternalAuth:GitHub:ClientId"]);
            options.ClientSecret = CredentialOrPlaceholder(configuration["ExternalAuth:GitHub:ClientSecret"]);
            ExternalAuthOptionsConfigurator.ConfigureGitHubDefaults(options);
        });

        return authBuilder;
    }

    /// <summary>
    /// ASP.NET Core OAuth validation rejects an empty ClientId. Host appsettings
    /// use empty strings for unused providers; those must not reach validation.
    /// </summary>
    private static string CredentialOrPlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "unconfigured" : value;
}
