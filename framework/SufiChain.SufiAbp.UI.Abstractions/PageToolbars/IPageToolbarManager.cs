namespace SufiChain.SufiAbp.UI.PageToolbars;

/// <summary>
/// Manages page toolbars and resolves their items.
/// </summary>
public interface IPageToolbarManager
{
    /// <summary>
    /// Gets all items for a page toolbar by invoking all contributors.
    /// </summary>
    /// <param name="toolbar">The page toolbar.</param>
    /// <returns>The resolved toolbar items sorted by order.</returns>
    Task<PageToolbarItem[]> GetItemsAsync(PageToolbar toolbar);
}
