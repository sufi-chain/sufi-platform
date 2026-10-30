namespace SufiChain.SufiPlatform.SufiAI.MCP.Tools;

public class MCPToolDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParameterSchema { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    /// <summary>Resolved effect name: Read or Write (sufi-chain#312). Missing values are Write.</summary>
    public string Effect { get; set; } = "Write";
}
