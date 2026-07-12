using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Localization.Dtos;

public class GetBusinessLocalizationKeyValuesInput
{
    [Required]
    [StringLength(128)]
    public string ResourceName { get; set; } = default!;

    [Required]
    [StringLength(512)]
    public string Key { get; set; } = default!;
}

public class SaveBusinessLocalizationKeyValuesInput
{
    [Required]
    [StringLength(128)]
    public string ResourceName { get; set; } = default!;

    [Required]
    [StringLength(512)]
    public string Key { get; set; } = default!;

    [Required]
    public Dictionary<string, string> Values { get; set; } = new();
}

public class BusinessLocalizationKeyValuesDto
{
    public string ResourceName { get; set; } = default!;

    public string Key { get; set; } = default!;

    public Dictionary<string, string> Values { get; set; } = new();

    public bool IsBusinessKey { get; set; }
}

public class BusinessLocalizationCultureDto
{
    public string CultureName { get; set; } = default!;

    public string DisplayName { get; set; } = default!;

    public bool IsRtl { get; set; }

    public bool IsDefault { get; set; }
}
