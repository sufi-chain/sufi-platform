namespace SufiChain.SufiAbp.UI.Toolbars;

/// <summary>
/// Manages and provides access to toolbars.
/// </summary>
public interface IToolbarManager
{
    /// <summary>
    /// Gets a toolbar by name, with all contributors applied.
    /// </summary>
    /// <param name="name">The toolbar name (e.g., StandardToolbars.Main).</param>
    /// <returns>The fully configured toolbar.</returns>
    Task<Toolbar> GetAsync(string name);
}
