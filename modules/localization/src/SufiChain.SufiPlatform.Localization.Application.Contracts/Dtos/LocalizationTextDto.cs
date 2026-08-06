using System;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Localization.Dtos;

public class LocalizationTextDto : AuditedEntityDto<Guid>
{
    public string ResourceName { get; set; } = default!;
    public string CultureName { get; set; } = default!;
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}

public class LocalizationTextWithBaseValueDto : LocalizationTextDto
{
    /// <summary>
    /// The base value from JSON files (if exists)
    /// </summary>
    public string? BaseValue { get; set; }

    /// <summary>
    /// Whether this is an override from database
    /// </summary>
    public bool IsOverride { get; set; }
}
