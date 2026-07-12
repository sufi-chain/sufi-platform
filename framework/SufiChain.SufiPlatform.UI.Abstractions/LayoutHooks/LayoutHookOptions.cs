namespace SufiChain.SufiPlatform.UI.LayoutHooks;

/// <summary>
/// Options for configuring layout hooks.
/// </summary>
public class LayoutHookOptions
{
    /// <summary>
    /// Dictionary of hook names to their component info list.
    /// </summary>
    public IDictionary<string, List<LayoutHookInfo>> Hooks { get; }

    /// <summary>
    /// Creates a new LayoutHookOptions.
    /// </summary>
    public LayoutHookOptions()
    {
        Hooks = new Dictionary<string, List<LayoutHookInfo>>();
    }

    /// <summary>
    /// Adds a component to a layout hook.
    /// </summary>
    /// <param name="hookName">The hook name (e.g., LayoutHooks.Body.First).</param>
    /// <param name="componentType">The component type to render.</param>
    /// <param name="layout">Optional layout name to restrict this hook to.</param>
    /// <returns>This options instance for chaining.</returns>
    public LayoutHookOptions Add(string hookName, Type componentType, string? layout = null)
    {
        if (!Hooks.TryGetValue(hookName, out var list))
        {
            list = new List<LayoutHookInfo>();
            Hooks[hookName] = list;
        }

        list.Add(new LayoutHookInfo(componentType, layout));
        return this;
    }

    /// <summary>
    /// Adds a component to a layout hook.
    /// </summary>
    /// <typeparam name="TComponent">The component type to render.</typeparam>
    /// <param name="hookName">The hook name (e.g., LayoutHooks.Body.First).</param>
    /// <param name="layout">Optional layout name to restrict this hook to.</param>
    /// <returns>This options instance for chaining.</returns>
    public LayoutHookOptions Add<TComponent>(string hookName, string? layout = null)
    {
        return Add(hookName, typeof(TComponent), layout);
    }
}
