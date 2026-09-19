using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

/// <summary>
/// Lightweight catalog item for cross-module hooshvare registry lookups.
/// </summary>
[Serializable]
public class HooshvareRegistryItemDto : EntityDto<Guid>
{
    public string? Key { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public HooshvareKind Kind { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public bool IsEnabled { get; set; }
}

/// <summary>
/// Runtime identity of a platform hooshvare resolved by key.
/// </summary>
[Serializable]
public class HooshvareRegistryRuntimeDto : EntityDto<Guid>
{
    public string? Key { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }
}
