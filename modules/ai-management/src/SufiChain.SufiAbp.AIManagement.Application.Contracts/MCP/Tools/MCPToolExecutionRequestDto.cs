using System.Collections.Generic;

namespace SufiChain.SufiAbp.AIManagement.MCP.Tools;

public class MCPToolExecutionRequestDto
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; set; } = new();
}
