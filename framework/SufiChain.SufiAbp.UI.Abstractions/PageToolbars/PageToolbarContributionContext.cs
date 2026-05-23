namespace SufiChain.SufiAbp.UI.PageToolbars;

/// <summary>
/// Context provided to page toolbar contributors.
/// </summary>
public class PageToolbarContributionContext
{
    /// <summary>
    /// The service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// The items being added to the toolbar.
    /// </summary>
    public PageToolbarItemList Items { get; }

    /// <summary>
    /// Creates a new PageToolbarContributionContext.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public PageToolbarContributionContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Items = new PageToolbarItemList();
    }
}
