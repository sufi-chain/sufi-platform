using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Tools;

public class MCPToolExecutionRequestDto
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; set; } = new();

    /// <summary>
    /// sufi-chain#384 slice 6b: conversation the call belongs to, when the caller knows it. Bound onto the
    /// call context so the audit row and any per-user external MCP token name the conversation.
    /// </summary>
    [StringLength(128)]
    public string? SessionId { get; set; }
}
