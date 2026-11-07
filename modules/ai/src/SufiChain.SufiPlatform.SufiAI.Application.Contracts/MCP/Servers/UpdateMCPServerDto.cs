using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Servers;

public class UpdateMCPServerDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(512)]
    public string? Endpoint { get; set; }
    
    [StringLength(256)]
    public string? Command { get; set; }
    
    [StringLength(2048)]
    public string? ArgumentsJson { get; set; }

    /// <summary>
    /// JSON object of environment names to values. Null keeps the stored map.
    /// An empty string value removes that name. <c>{}</c> clears the map.
    /// Values are encrypted and are never written back to this DTO.
    /// </summary>
    [StringLength(8192)]
    public string? EnvironmentJson { get; set; }

    /// <summary>
    /// sufi-chain#384 slice 6b: <c>TenantLevel</c> or <c>UserScoped</c>. Null keeps the stored value, so a
    /// caller that omits the field can never silently downgrade a user-scoped server.
    /// </summary>
    public string? BindingMode { get; set; }

    /// <summary>
    /// sufi-chain#557: <c>STDIO</c>, <c>SSE</c>, or <c>HTTP</c>. Null keeps the stored transport.
    /// </summary>
    public string? TransportType { get; set; }
}
