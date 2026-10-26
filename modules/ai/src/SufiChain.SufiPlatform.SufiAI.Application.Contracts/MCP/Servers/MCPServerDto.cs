using System;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Servers;

public class MCPServerDto : FullAuditedEntityDto<Guid>
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TransportType { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public string? Command { get; set; }
    public string? ArgumentsJson { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>
    /// sufi-chain#384 slice 6b: <c>TenantLevel</c> or <c>UserScoped</c>.
    /// </summary>
    public string BindingMode { get; set; } = "TenantLevel";

    public DateTime? LastConnectedAt { get; set; }
    public string? LastConnectionError { get; set; }
}
