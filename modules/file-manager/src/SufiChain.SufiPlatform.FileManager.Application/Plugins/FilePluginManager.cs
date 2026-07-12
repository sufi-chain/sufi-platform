using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.Plugins;

public class FilePluginManager : IFilePluginManager, ISingletonDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FilePluginManager> _logger;
    private readonly List<IFilePlugin> _plugins = new();
    private bool _isInitialized;

    public FilePluginManager(
        IServiceProvider serviceProvider,
        ILogger<FilePluginManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DiscoverPluginsAsync()
    {
        if (_isInitialized)
        {
            _logger.LogInformation("Plugins already initialized, skipping discovery");
            return;
        }

        _logger.LogInformation("Starting plugin discovery...");
        
        try
        {
            var plugins = _serviceProvider.GetServices<IFilePlugin>();
            var pluginsList = plugins.ToList();
            
            _logger.LogInformation($"Found {pluginsList.Count} plugin(s)");

            foreach (var plugin in pluginsList)
            {
                try
                {
                    _logger.LogInformation($"Initializing plugin: {plugin.Name} v{plugin.Version}");
                    await plugin.InitializeAsync();
                    _plugins.Add(plugin);
                    _logger.LogInformation($"Successfully loaded plugin: {plugin.Name} - {plugin.Description}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to initialize plugin: {plugin.Name}");
                }
            }

            _isInitialized = true;
            _logger.LogInformation($"Plugin discovery completed. {_plugins.Count} plugin(s) loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin discovery");
            throw;
        }
    }

    public IEnumerable<T> GetPlugins<T>() where T : IFilePlugin
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("Plugins not initialized. Call DiscoverPluginsAsync first");
        }

        return _plugins.OfType<T>();
    }

    public T GetPlugin<T>(string name) where T : IFilePlugin
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("Plugins not initialized. Call DiscoverPluginsAsync first");
        }

        var plugin = _plugins.OfType<T>().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        
        if (plugin == null)
        {
            _logger.LogWarning($"Plugin not found: {name}");
        }

        return plugin;
    }

    public IEnumerable<IFilePlugin> GetAllPlugins()
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("Plugins not initialized. Call DiscoverPluginsAsync first");
        }

        return _plugins.AsReadOnly();
    }

    public bool IsPluginLoaded(string name)
    {
        return _plugins.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
