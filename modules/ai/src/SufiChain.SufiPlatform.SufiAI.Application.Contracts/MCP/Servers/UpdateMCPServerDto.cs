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
    
    [StringLength(4096)]
    public string? MetadataJson { get; set; }
}
