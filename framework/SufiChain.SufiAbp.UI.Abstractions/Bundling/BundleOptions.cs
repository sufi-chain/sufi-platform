namespace SufiChain.SufiAbp.UI.Bundling;

/// <summary>
/// Options for configuring bundles.
/// </summary>
public class BundleOptions
{
    /// <summary>
    /// Style bundles configuration.
    /// </summary>
    public BundleConfiguration StyleBundles { get; }

    /// <summary>
    /// Script bundles configuration.
    /// </summary>
    public BundleConfiguration ScriptBundles { get; }

    /// <summary>
    /// Creates a new BundleOptions.
    /// </summary>
    public BundleOptions()
    {
        StyleBundles = new BundleConfiguration();
        ScriptBundles = new BundleConfiguration();
    }
}

/// <summary>
/// Configuration for a type of bundles (styles or scripts).
/// </summary>
public class BundleConfiguration
{
    private readonly Dictionary<string, List<string>> _bundles = new();

    /// <summary>
    /// Adds a file to a bundle.
    /// </summary>
    /// <param name="bundleName">The bundle name.</param>
    /// <param name="filePath">The file path to add.</param>
    public void Add(string bundleName, string filePath)
    {
        if (!_bundles.TryGetValue(bundleName, out var files))
        {
            files = new List<string>();
            _bundles[bundleName] = files;
        }
        files.Add(filePath);
    }

    /// <summary>
    /// Gets the files in a bundle.
    /// </summary>
    /// <param name="bundleName">The bundle name.</param>
    /// <returns>The list of file paths.</returns>
    public IReadOnlyList<string> GetFiles(string bundleName)
    {
        return _bundles.TryGetValue(bundleName, out var files)
            ? files.AsReadOnly()
            : Array.Empty<string>();
    }
}
