using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

public class WorkspaceDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public AIProviderType Provider { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool HasApiKey { get; set; }
    public string? ApiBaseUrl { get; set; }
    public string? SystemPrompt { get; set; }
    public float Temperature { get; set; }
    public int MaxTokens { get; set; }
    public OpenAIApiMode OpenAIApiMode { get; set; }
    public decimal? InputCostPer1KTokens { get; set; }
    public decimal? OutputCostPer1KTokens { get; set; }
    public bool IsActive { get; set; }
    public bool HasEmbedderConfig { get; set; }
    public bool HasVectorStoreConfig { get; set; }
}
