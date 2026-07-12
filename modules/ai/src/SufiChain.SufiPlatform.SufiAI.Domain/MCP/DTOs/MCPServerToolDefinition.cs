namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Tool definition from an external MCP server.
/// </summary>
public class MCPServerToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParameterSchema { get; set; } = string.Empty;
}
