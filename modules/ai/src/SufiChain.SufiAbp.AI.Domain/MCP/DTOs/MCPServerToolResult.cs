namespace SufiChain.SufiAbp.AI.MCP.Abstractions;

/// <summary>
/// Result from calling a tool on an external MCP server.
/// </summary>
public class MCPServerToolResult
{
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
}
