using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.LocalizationManagement.Dtos;

public class ImportLocalizationTextsDto
{
    [Required]
    [StringLength(128)]
    public string ResourceName { get; set; } = default!;

    [Required]
    [StringLength(16)]
    public string CultureName { get; set; } = default!;

    /// <summary>
    /// JSON content in ABP format: { "culture": "en", "texts": { "Key": "Value" } }
    /// </summary>
    [Required]
    public string JsonContent { get; set; } = default!;

    /// <summary>
    /// Whether to overwrite existing translations
    /// </summary>
    public bool OverwriteExisting { get; set; } = true;
}

public class ExportLocalizationTextsDto
{
    [Required]
    [StringLength(128)]
    public string ResourceName { get; set; } = default!;

    [Required]
    [StringLength(16)]
    public string CultureName { get; set; } = default!;

    /// <summary>
    /// Whether to include only database overrides (true) or merged with base translations (false)
    /// </summary>
    public bool DatabaseOnlyOverrides { get; set; } = false;
}

public class ImportResultDto
{
    public int TotalKeys { get; set; }
    public int ImportedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
