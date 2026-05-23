using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace SufiChain.SufiAbp.UI.Toolbars;

/// <summary>
/// Context provided to toolbar contributors for configuration.
/// </summary>
public interface IToolbarConfigurationContext
{
    /// <summary>
    /// The service provider for resolving dependencies.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// The toolbar being configured.
    /// </summary>
    Toolbar Toolbar { get; }

    /// <summary>
    /// The authorization service for checking permissions.
    /// </summary>
    IAuthorizationService AuthorizationService { get; }

    /// <summary>
    /// The string localizer factory for creating localizers.
    /// </summary>
    IStringLocalizerFactory StringLocalizerFactory { get; }

    /// <summary>
    /// Checks if the current user has the specified permission.
    /// </summary>
    /// <param name="policyName">The policy/permission name to check.</param>
    /// <returns>True if the permission is granted.</returns>
    Task<bool> IsGrantedAsync(string policyName);

    /// <summary>
    /// Gets the default string localizer.
    /// </summary>
    /// <returns>The default localizer, or null if not configured.</returns>
    IStringLocalizer? GetDefaultLocalizer();

    /// <summary>
    /// Gets a string localizer for the specified resource type.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <returns>The string localizer.</returns>
    IStringLocalizer GetLocalizer<T>();

    /// <summary>
    /// Gets a string localizer for the specified resource type.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <returns>The string localizer.</returns>
    IStringLocalizer GetLocalizer(Type resourceType);
}
