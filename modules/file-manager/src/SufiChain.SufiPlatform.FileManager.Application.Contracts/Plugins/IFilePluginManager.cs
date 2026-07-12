using System.Collections.Generic;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.Plugins;

/// <summary>
/// Interface for managing file plugins
/// </summary>
public interface IFilePluginManager
{
    /// <summary>
    /// Discovers and initializes all available plugins
    /// </summary>
    Task DiscoverPluginsAsync();

    /// <summary>
    /// Gets all plugins of the specified type
    /// </summary>
    IEnumerable<T> GetPlugins<T>() where T : IFilePlugin;

    /// <summary>
    /// Gets a specific plugin by name
    /// </summary>
    T GetPlugin<T>(string name) where T : IFilePlugin;

    /// <summary>
    /// Gets all loaded plugins
    /// </summary>
    IEnumerable<IFilePlugin> GetAllPlugins();

    /// <summary>
    /// Checks if a plugin is loaded
    /// </summary>
    bool IsPluginLoaded(string name);
}
