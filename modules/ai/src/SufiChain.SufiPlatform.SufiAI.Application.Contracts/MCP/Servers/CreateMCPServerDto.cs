using System.ComponentModel.DataAnnotations;

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
}
