namespace SufiChain.SufiPlatform.UI.Navigation;

/// <summary>
/// Manages application menus.
/// </summary>
public interface IMenuManager
{
    /// <summary>
    /// Gets a menu by name.
    /// </summary>
    /// <param name="name">The menu name (e.g., StandardMenus.Main).</param>
    /// <returns>The fully configured menu.</returns>
    Task<ApplicationMenu> GetAsync(string name);

    /// <summary>
    /// Gets the main menu.
    /// </summary>
    /// <returns>The main application menu.</returns>
    Task<ApplicationMenu> GetMainMenuAsync();
}
