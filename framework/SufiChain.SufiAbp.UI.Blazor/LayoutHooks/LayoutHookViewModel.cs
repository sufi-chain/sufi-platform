using SufiChain.SufiAbp.UI.LayoutHooks;

namespace SufiChain.SufiAbp.UI.Blazor.LayoutHooks;

/// <summary>
/// View model for layout hooks.
/// </summary>
public class LayoutHookViewModel
{
    /// <summary>
    /// The hooks to render.
    /// </summary>
    public LayoutHookInfo[] Hooks { get; }

    /// <summary>
    /// The current layout name.
    /// </summary>
    public string? Layout { get; }

    /// <summary>
    /// Creates a new LayoutHookViewModel.
    /// </summary>
    public LayoutHookViewModel(LayoutHookInfo[] hooks, string? layout)
    {
        Hooks = hooks;
        Layout = layout;
    }
}
