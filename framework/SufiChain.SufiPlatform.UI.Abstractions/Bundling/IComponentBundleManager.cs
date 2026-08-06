namespace SufiChain.SufiPlatform.UI.Bundling;

/// <summary>
/// Manages component bundles for styles and scripts.
/// </summary>
public interface IComponentBundleManager
{
    /// <summary>
    /// Gets the CSS file paths for a bundle.
    /// </summary>
    /// <param name="bundleName">The bundle name.</param>
    /// <returns>The list of CSS file paths.</returns>
    Task<IReadOnlyList<string>> GetStyleBundleFilesAsync(string bundleName);

    /// <summary>
    /// Gets the JavaScript file paths for a bundle.
    /// </summary>
    /// <param name="bundleName">The bundle name.</param>
    /// <returns>The list of JavaScript file paths.</returns>
    Task<IReadOnlyList<string>> GetScriptBundleFilesAsync(string bundleName);
}
