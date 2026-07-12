namespace SufiChain.SufiPlatform.UI.LayoutHooks;

/// <summary>
/// Information about a component to be injected at a layout hook point.
/// </summary>
public class LayoutHookInfo
{
    /// <summary>
    /// The component type to render.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// The layout name to apply this hook to.
    /// Null indicates the hook applies to all layouts.
    /// </summary>
    public string? Layout { get; }

    /// <summary>
    /// Creates a new LayoutHookInfo.
    /// </summary>
    /// <param name="componentType">The component type to render.</param>
    /// <param name="layout">Optional layout name to restrict this hook to.</param>
    public LayoutHookInfo(Type componentType, string? layout = null)
    {
        ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        Layout = layout;
    }
}
