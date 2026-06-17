using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

public class CreateWorkspaceDto
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
    
    [StringLength(4096)]
    public string? SystemPrompt { get; set; }
    
    [Range(0.0f, 2.0f)]
    public float Temperature { get; set; } = 0.7f;
    
    [Range(1, 100000)]
    public int MaxTokens { get; set; } = 2000;

    public OpenAIApiMode OpenAIApiMode { get; set; } = OpenAIApiMode.ChatCompletions;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? InputCostPer1MTokens { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? OutputCostPer1MTokens { get; set; }
    
    public EmbedderConfigDto? EmbedderConfig { get; set; }
    public VectorStoreConfigDto? VectorStoreConfig { get; set; }
}

public class EmbedderConfigDto
{
    public AIProviderType Provider { get; set; }
    public string Model { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? ApiBaseUrl { get; set; }
}

public class VectorStoreConfigDto
{
    public VectorStoreType Type { get; set; }
    public string? ConnectionString { get; set; }
    public string CollectionName { get; set; } = "ai_documents";
    public int Dimensions { get; set; } = 1536;
}
