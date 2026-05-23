using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace SufiChain.SufiAbp.UI.Blazor.Components;

/// <summary>
/// Component for rendering dynamic components configured via DynamicLayoutComponentOptions.
/// Typically placed at the end of layouts to render modals, dialogs, etc.
/// </summary>
public partial class SufiAbpDynamicLayoutComponent : ComponentBase
{
    [Inject]
    protected IOptions<DynamicLayoutComponentOptions> DynamicLayoutComponentOptions { get; set; } = default!;
}
