using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.UI.Toolbars;

namespace SufiChain.SufiAbp.UI.Services.Toolbars;

/// <summary>
/// Default implementation of IToolbarManager.
/// </summary>
public class DefaultToolbarManager : IToolbarManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ToolbarOptions _options;

    public DefaultToolbarManager(
        IServiceProvider serviceProvider,
        IOptions<ToolbarOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<Toolbar> GetAsync(string name)
    {
        var toolbar = new Toolbar(name);
        var context = new ToolbarConfigurationContext(toolbar, _serviceProvider);

        // Get all contributors from options and from DI
        var contributors = _options.Contributors
            .Concat(_serviceProvider.GetServices<IToolbarContributor>())
            .ToList();

        foreach (var contributor in contributors)
        {
            await contributor.ConfigureToolbarAsync(context);
        }

        // Sort items by order
        toolbar.Items.Sort((a, b) => a.Order.CompareTo(b.Order));

        return toolbar;
    }
}
