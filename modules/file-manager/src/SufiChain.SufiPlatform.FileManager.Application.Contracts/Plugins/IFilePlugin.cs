using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.Plugins;

/// <summary>
/// Base interface for all file management plugins
/// </summary>
public interface IFilePlugin
{
    /// <summary>
    /// Gets the unique name of the plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the plugin
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the description of the plugin
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Initializes the plugin. Called when the plugin is first loaded.
    /// </summary>
    Task InitializeAsync();
}
