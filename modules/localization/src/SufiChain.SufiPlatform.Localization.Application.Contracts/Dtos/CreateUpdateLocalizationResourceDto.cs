using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Localization.Dtos;

public class CreateUpdateLocalizationResourceDto
{
    [Required]
    [StringLength(128)]
    public string ResourceName { get; set; } = default!;

    [StringLength(16)]
    public string DefaultCulture { get; set; } = "en";

    [StringLength(256)]
    public string? DisplayName { get; set; }

    public bool IsEnabled { get; set; } = true;

    public List<string> BaseResourceNames { get; set; } = new();
}
