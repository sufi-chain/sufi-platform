using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Servers;

public class CreateMCPServerDto
{
    [Required]
    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string TransportType { get; set; } = string.Empty;
    
    [StringLength(512)]
    public string? Endpoint { get; set; }
    
    [StringLength(256)]
    public string? Command { get; set; }
    
    [StringLength(2048)]
    public string? ArgumentsJson { get; set; }

    /// <summary>
    /// JSON object of environment names to values for a STDIO server.
    /// Values are encrypted and are not returned. They are not command arguments.
    /// Null or blank stores nothing. <c>{}</c> clears stored values.
    /// Auditing skips this property so the plaintext is not stored in the action parameters.
    /// </summary>
    [DisableAuditing]
    [StringLength(8192)]
    public string? EnvironmentJson { get; set; }

    /// <summary>
    /// sufi-chain#384 slice 6b: <c>TenantLevel</c> (default) or <c>UserScoped</c>. UserScoped is rejected
    /// with STDIO.
    /// </summary>
    public string BindingMode { get; set; } = "TenantLevel";
}
