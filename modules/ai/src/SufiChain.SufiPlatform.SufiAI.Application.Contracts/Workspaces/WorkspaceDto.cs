using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public AIProviderType Provider { get; set; }
    public string Model { get; set; } = string.Empty;
    public string? ModelDisplayName { get; set; }
    public string? DecisionsModelId { get; set; }
    public bool HasApiKey { get; set; }
    public string? ApiBaseUrl { get; set; }
    public decimal? InputCostPer1MTokens { get; set; }
    public decimal? OutputCostPer1MTokens { get; set; }
    public bool IsActive { get; set; }
    public bool IsInherited { get; set; }
    public bool IsDefault { get; set; }
    public Guid? SourceWorkspaceId { get; set; }
    public Guid? AssignmentId { get; set; }
    public List<WorkspaceGuardrailDto> Guardrails { get; set; } = new();
}
