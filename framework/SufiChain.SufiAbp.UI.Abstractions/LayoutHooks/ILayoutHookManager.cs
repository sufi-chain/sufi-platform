namespace SufiChain.SufiAbp.UI.LayoutHooks;

/// <summary>
/// Manages layout hooks and provides components for hook points.
/// </summary>
public interface ILayoutHookManager
{
    /// <summary>
    /// Gets all components registered for a specific hook.
    /// </summary>
    /// <param name="hookName">The hook name (e.g., LayoutHooks.Body.First).</param>
    /// <param name="layoutName">Optional layout name to filter hooks.</param>
    /// <returns>The list of components to render at this hook point.</returns>
    IReadOnlyList<LayoutHookInfo> GetHooks(string hookName, string? layoutName = null);
}
