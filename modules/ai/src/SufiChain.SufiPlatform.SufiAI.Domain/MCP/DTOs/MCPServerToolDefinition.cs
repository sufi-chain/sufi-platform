namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Tool definition from an external MCP server.
/// </summary>
public class MCPServerToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParameterSchema { get; set; } = string.Empty;

    /// <summary>
    /// MCP <c>annotations.readOnlyHint</c>, when the server sent one.
    /// This is not an effect. An admin policy decides Read or Write.
    /// </summary>
    public bool? ReadOnlyHint { get; set; }
}
