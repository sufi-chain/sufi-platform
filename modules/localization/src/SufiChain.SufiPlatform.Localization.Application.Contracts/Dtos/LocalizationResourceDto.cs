using System;
using System.Collections.Generic;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Localization.Dtos;

public class LocalizationResourceDto : AuditedEntityDto<Guid>
{
    public string ResourceName { get; set; } = default!;
    public string DefaultCulture { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public string? DisplayName { get; set; }
    public List<string> BaseResourceNames { get; set; } = new();
}

public class LocalizationResourceSummaryDto
{
    public string ResourceName { get; set; } = default!;
    public string? DisplayName { get; set; }
    public bool IsEnabled { get; set; }
    public int TextCount { get; set; }
    public List<string> SupportedCultures { get; set; } = new();
}
