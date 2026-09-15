using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class UpdateWorkspaceDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public AIProviderType Provider { get; set; }
    
    [Required]
    [StringLength(256)]
    public string Model { get; set; } = string.Empty;
    
    [StringLength(512)]
    public string? ApiKey { get; set; }
    
    [StringLength(512)]
    public string? ApiBaseUrl { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? InputCostPer1MTokens { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? OutputCostPer1MTokens { get; set; }
    
    public bool IsActive { get; set; } = true;
}
