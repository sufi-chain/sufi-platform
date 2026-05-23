namespace SufiChain.SufiAbp.UI.Toolbars;

/// <summary>
/// Interface for contributing items to toolbars.
/// Implementations can add components to specific toolbars.
/// </summary>
public interface IToolbarContributor
{
    /// <summary>
    /// Configures the toolbar by adding items.
    /// </summary>
    /// <param name="context">The configuration context containing the toolbar and services.</param>
    Task ConfigureToolbarAsync(IToolbarConfigurationContext context);
}
