using System;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.AI.MCP.Servers;

public class CreateMCPServerDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Guid WorkspaceId { get; set; }
    
    [Required]
    public string TransportType { get; set; } = string.Empty;
    
    [StringLength(512)]
    public string? Endpoint { get; set; }
    
    [StringLength(256)]
    public string? Command { get; set; }
    
    [StringLength(2048)]
    public string? ArgumentsJson { get; set; }
    
    [StringLength(4096)]
    public string? MetadataJson { get; set; }
}
