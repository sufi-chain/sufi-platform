using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.UI.Toolbars;

namespace SufiChain.SufiPlatform.UI.Services.Toolbars;

/// <summary>
/// Default implementation of IToolbarConfigurationContext.
/// </summary>
public class ToolbarConfigurationContext : IToolbarConfigurationContext
{
    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc/>
    public Toolbar Toolbar { get; }

    private IAuthorizationService? _authorizationService;
    private IStringLocalizerFactory? _stringLocalizerFactory;

    /// <inheritdoc/>
    public IAuthorizationService AuthorizationService =>
        _authorizationService ??= ServiceProvider.GetRequiredService<IAuthorizationService>();

    /// <inheritdoc/>
    public IStringLocalizerFactory StringLocalizerFactory =>
        _stringLocalizerFactory ??= ServiceProvider.GetRequiredService<IStringLocalizerFactory>();

    public ToolbarConfigurationContext(Toolbar toolbar, IServiceProvider serviceProvider)
    {
        Toolbar = toolbar ?? throw new ArgumentNullException(nameof(toolbar));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc/>
    public async Task<bool> IsGrantedAsync(string policyName)
    {
        var result = await AuthorizationService.AuthorizeAsync(null!, policyName);
        return result.Succeeded;
    }

    /// <inheritdoc/>
    public IStringLocalizer? GetDefaultLocalizer()
    {
        // Return null by default - implementations can override
        return null;
    }

    /// <inheritdoc/>
    public IStringLocalizer GetLocalizer<T>()
    {
        return StringLocalizerFactory.Create(typeof(T));
    }

    /// <inheritdoc/>
    public IStringLocalizer GetLocalizer(Type resourceType)
    {
        return StringLocalizerFactory.Create(resourceType);
    }
}
