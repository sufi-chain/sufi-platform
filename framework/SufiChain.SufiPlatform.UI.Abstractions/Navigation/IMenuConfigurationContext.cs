using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace SufiChain.SufiPlatform.UI.Navigation;

/// <summary>
/// Context provided to menu contributors for configuration.
/// </summary>
public interface IMenuConfigurationContext
{
    /// <summary>
    /// The service provider for resolving dependencies.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// The menu being configured.
    /// </summary>
    ApplicationMenu Menu { get; }

    /// <summary>
    /// The authorization service for checking permissions.
    /// </summary>
    IAuthorizationService AuthorizationService { get; }

    /// <summary>
    /// The string localizer factory for creating localizers.
    /// </summary>
    IStringLocalizerFactory StringLocalizerFactory { get; }
}
