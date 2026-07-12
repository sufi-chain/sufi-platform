using System;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Tools;

public class MCPToolExecutionResultDto
{
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExceptionDetails { get; set; }
    public long ExecutionTimeMs { get; set; }
    public DateTime ExecutedAt { get; set; }
}
