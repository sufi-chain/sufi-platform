namespace SufiChain.SufiPlatform.SufiAI.MCP.Tools;

public class MCPToolResolutionRequestDto
{
    public List<string> ToolNames { get; set; } = new();
}

public class MCPToolResolutionResultDto
{
    public List<MCPToolDto> Tools { get; set; } = new();
    public List<MCPToolResolutionDiagnosticDto> Diagnostics { get; set; } = new();
}

public class MCPToolResolutionDiagnosticDto
{
    public string ToolName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
