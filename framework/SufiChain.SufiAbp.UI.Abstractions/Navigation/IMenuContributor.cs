namespace SufiChain.SufiAbp.UI.Navigation;

/// <summary>
/// Interface for contributing items to menus.
/// </summary>
public interface IMenuContributor
{
    /// <summary>
    /// Configures the menu by adding items.
    /// </summary>
    /// <param name="context">The configuration context containing the menu and services.</param>
    Task ConfigureMenuAsync(MenuConfigurationContext context);
}
