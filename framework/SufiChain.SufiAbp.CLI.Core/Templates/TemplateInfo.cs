namespace SufiChain.SufiAbp.CLI.Templates;

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
