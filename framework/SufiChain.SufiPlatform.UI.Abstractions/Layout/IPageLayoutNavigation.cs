namespace SufiChain.SufiPlatform.UI.Layout;

/// <summary>
/// Marks the start of a client navigation so <see cref="IPageLayout.Reset"/> can keep
/// title and breadcrumbs the incoming page already published.
/// </summary>
public interface IPageLayoutNavigation
{
    /// <summary>
    /// Opens a navigation generation. Content written after this call survives the next <see cref="IPageLayout.Reset"/>.
    /// </summary>
    void BeginNavigation();
}
