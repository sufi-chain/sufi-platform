using System.Collections.Generic;

namespace SufiChain.SufiPlatform.FileManager.Plugins.Dtos;

/// <summary>
/// Generic parameters for custom filters
/// </summary>
public class FilterParameters
{
    public string FilterName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();

    public T GetParameter<T>(string key, T defaultValue = default)
    {
        if (Parameters.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public void SetParameter<T>(string key, T value)
    {
        Parameters[key] = value;
    }
}

