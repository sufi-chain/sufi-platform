using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.Bundling;

namespace SufiChain.SufiPlatform.UI.Services.Bundling;

/// <summary>
/// Default implementation of IComponentBundleManager.
/// </summary>
public class DefaultComponentBundleManager : IComponentBundleManager
{
    private readonly BundleOptions _options;

    public DefaultComponentBundleManager(IOptions<BundleOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> GetStyleBundleFilesAsync(string bundleName)
    {
        var files = _options.StyleBundles.GetFiles(bundleName);
        return Task.FromResult(files);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> GetScriptBundleFilesAsync(string bundleName)
    {
        var files = _options.ScriptBundles.GetFiles(bundleName);
        return Task.FromResult(files);
    }
}
