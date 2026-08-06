using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.UI.PageToolbars;

namespace SufiChain.SufiPlatform.UI.Services.PageToolbars;

/// <summary>
/// Default implementation of IPageToolbarManager.
/// </summary>
public class DefaultPageToolbarManager : IPageToolbarManager
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultPageToolbarManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task<PageToolbarItem[]> GetItemsAsync(PageToolbar toolbar)
    {
        var context = new PageToolbarContributionContext(_serviceProvider);

        foreach (var contributor in toolbar.Contributors)
        {
            await contributor.ContributeAsync(context);
        }

        return context.Items
            .OrderBy(x => x.Order)
            .ToArray();
    }
}
