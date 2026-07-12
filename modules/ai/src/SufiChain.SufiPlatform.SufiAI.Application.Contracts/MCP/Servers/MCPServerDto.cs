using System;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Servers;

public class MCPServerDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public string TransportType { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public string? Command { get; set; }
    public string? ArgumentsJson { get; set; }
    public bool IsEnabled { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime? LastConnectedAt { get; set; }
    public string? LastConnectionError { get; set; }
}
