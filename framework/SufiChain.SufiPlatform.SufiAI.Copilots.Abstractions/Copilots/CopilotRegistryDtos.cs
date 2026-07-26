using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Lightweight catalog item for cross-module copilot registry lookups.
/// </summary>
[Serializable]
public class CopilotRegistryItemDto : EntityDto<Guid>
{
    public string? Key { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public CopilotKind Kind { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public bool IsEnabled { get; set; }
}

/// <summary>
/// Runtime identity of a platform copilot resolved by key.
/// </summary>
[Serializable]
public class CopilotRegistryRuntimeDto : EntityDto<Guid>
{
    public string? Key { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }
}
