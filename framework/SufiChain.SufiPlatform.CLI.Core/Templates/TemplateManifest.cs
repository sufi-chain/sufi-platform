using System.Text.Json;

namespace SufiChain.SufiPlatform.CLI.Templates;

/// <summary>
/// Manifest file for embedded templates, mapping resource names to file paths.
/// </summary>
public class TemplateManifest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public List<TemplateFileEntry> Files { get; set; } = new();
    
    public static TemplateManifest? FromJson(string json)
    {
        return JsonSerializer.Deserialize<TemplateManifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}

public class TemplateFileEntry
{
    /// <summary>
    /// The embedded resource name.
    /// </summary>
    public string ResourceName { get; set; } = "";
    
    /// <summary>
    /// The original file path relative to the template root.
    /// </summary>
    public string FilePath { get; set; } = "";
    
    /// <summary>
    /// Whether this is a binary file.
    /// </summary>
    public bool IsBinary { get; set; }
}
