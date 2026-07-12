using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.LayoutHooks;

namespace SufiChain.SufiPlatform.UI.Blazor.LayoutHooks;

/// <summary>
/// Component for rendering layout hooks at specific points in the layout.
/// </summary>
public partial class SufiLayoutHook : ComponentBase
{
    /// <summary>
    /// The name of the hook (e.g., LayoutHooks.Body.First).
    /// </summary>
    [Parameter]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Optional layout name to filter hooks for a specific layout.
    /// </summary>
    [Parameter]
    public string? Layout { get; set; }

    [Inject]
    protected ILayoutHookManager LayoutHookManager { get; set; } = default!;

    protected LayoutHookViewModel LayoutHookViewModel { get; private set; } = new(Array.Empty<LayoutHookInfo>(), null);

    protected override Task OnInitializedAsync()
    {
        var hooks = LayoutHookManager.GetHooks(Name, Layout)
            .Where(IsComponentBase)
            .ToArray();

        LayoutHookViewModel = new LayoutHookViewModel(hooks, Layout);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if the hook's component type is a valid Blazor component.
    /// </summary>
    protected virtual bool IsComponentBase(LayoutHookInfo layoutHook)
    {
        return typeof(ComponentBase).IsAssignableFrom(layoutHook.ComponentType);
    }
}
