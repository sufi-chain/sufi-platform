using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace SufiChain.SufiAbp.UI.Navigation;

/// <summary>
/// Default implementation of IMenuConfigurationContext.
/// </summary>
public class MenuConfigurationContext : IMenuConfigurationContext
{
    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc/>
    public ApplicationMenu Menu { get; }

    private IAuthorizationService? _authorizationService;
    private IStringLocalizerFactory? _stringLocalizerFactory;

    /// <inheritdoc/>
    public IAuthorizationService AuthorizationService =>
        _authorizationService ??= ServiceProvider.GetRequiredService<IAuthorizationService>();

    /// <inheritdoc/>
    public IStringLocalizerFactory StringLocalizerFactory =>
        _stringLocalizerFactory ??= ServiceProvider.GetRequiredService<IStringLocalizerFactory>();

    /// <summary>
    /// Creates a new MenuConfigurationContext.
    /// </summary>
    /// <param name="menu">The menu being configured.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public MenuConfigurationContext(ApplicationMenu menu, IServiceProvider serviceProvider)
    {
        Menu = menu ?? throw new ArgumentNullException(nameof(menu));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Gets a string localizer for the specified resource type.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <returns>The string localizer.</returns>
    public IStringLocalizer GetLocalizer<T>()
    {
        return StringLocalizerFactory.Create(typeof(T));
    }

    /// <summary>
    /// Gets a string localizer for the specified resource type.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <returns>The string localizer.</returns>
    public IStringLocalizer GetLocalizer(Type resourceType)
    {
        return StringLocalizerFactory.Create(resourceType);
    }
}
