using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.LayoutHooks;

namespace SufiChain.SufiPlatform.UI.Services.LayoutHooks;

/// <summary>
/// Default implementation of ILayoutHookManager.
/// </summary>
public class DefaultLayoutHookManager : ILayoutHookManager
{
    private readonly LayoutHookOptions _options;

    public DefaultLayoutHookManager(IOptions<LayoutHookOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public IReadOnlyList<LayoutHookInfo> GetHooks(string hookName, string? layoutName = null)
    {
        if (!_options.Hooks.TryGetValue(hookName, out var hooks))
        {
            return Array.Empty<LayoutHookInfo>();
        }

        if (layoutName == null)
        {
            return hooks.AsReadOnly();
        }

        // Filter hooks that apply to all layouts (Layout == null) or the specific layout
        return hooks
            .Where(h => h.Layout == null || h.Layout == layoutName)
            .ToList()
            .AsReadOnly();
    }
}
