namespace SufiChain.SufiPlatform.CLI.Templates;

/// <summary>
/// Information about an available template.
/// </summary>
public class TemplateInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Source { get; set; } = ""; // "embedded" or "filesystem"
    public string Path { get; set; } = ""; // File system path if applicable
    public List<string> SupportedDatabaseProviders { get; set; } = new();
    public List<string> SupportedArchitectures { get; set; } = new();
}

/// <summary>
/// CDN version manifest structure for dynamic template version discovery.
/// </summary>
public class CdnVersionManifest
{
    public string Version { get; set; } = "";
    public Dictionary<string, CdnTemplateInfo> Templates { get; set; } = new();
}

/// <summary>
/// CDN template information including download URL and integrity hash.
/// </summary>
public class CdnTemplateInfo
{
    public string Url { get; set; } = "";
    public long Size { get; set; }
    public string Sha256 { get; set; } = "";
}
