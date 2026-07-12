using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public AIProviderType Provider { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool HasApiKey { get; set; }
    public string? ApiBaseUrl { get; set; }
    public string? SystemPrompt { get; set; }
    public float Temperature { get; set; }
    public int MaxContextTokens { get; set; }
    public OpenAIApiMode OpenAIApiMode { get; set; }
    public decimal? InputCostPer1MTokens { get; set; }
    public decimal? OutputCostPer1MTokens { get; set; }
    public bool IsActive { get; set; }
    public bool HasEmbedderConfig { get; set; }
    public bool HasVectorStoreConfig { get; set; }
    public int EnabledMCPToolCount { get; set; }
}
